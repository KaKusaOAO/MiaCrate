using Mochi.Utils;

namespace MiaCrate.Data;

public interface IComp : IFuncPointFree
{
    public IPointFree<IFunction>[] Functions { get; }
}

public interface ICompIn<T> : IComp {}
public interface ICompOut<T> : IComp {}
public interface IComp<TIn, TOut> : ICompIn<TIn>, ICompOut<TOut>, IFuncPointFree<TIn, TOut> {}

public sealed class Comp<TIn, TOut> : PointFree<IFunction<TIn, TOut>>, IComp<TIn, TOut>
{
    public IPointFree<IFunction>[] Functions { get; }
    public override IType<IFunction<TIn, TOut>> Type { get; }
    
    public override Func<IDynamicOps, IFunction<TIn, TOut>> Eval()
    {
        return ops => Function.Create<TIn, TOut>(input =>
        {
            object value = input!;
            for (var i = Functions.Length - 1; i >= 0; i--)
            {
                var f = Functions[i];
                value = f.EvalCached()(ops).Apply(value)!;
            }

            return (TOut)value;
        });
    }

    public override string ToString(int level)
    {
        throw new NotImplementedException();
    }

    public Comp(params IPointFree<IFunction>[] functions)
    {
        Functions = functions;
        var first = Functions.First();
        var last = Functions.Last();
        Type = Dsl.Func(
            ((IFuncTypeIn<TIn>) first.Type).First,
            ((IFuncTypeOut<TOut>) last.Type).Second
        );
    }
    
    public Comp(IPointFree<IFunction>[] functions, IType<IFunction<TIn, TOut>> type)
    {
        Functions = functions;
        Type = type;
    }

    public override IOptional<IPointFree<IFunction<TIn, TOut>>> All(IPointFreeRule rule)
    {
        var newFunctions = new List<IPointFree<IFunction>>();
        var rewritten = false;
        
        foreach (var function in Functions)
        {
            var rewrite = rule.RewriteOrNop(function);
            if (!ReferenceEquals(rewrite, function))
            {
                rewritten = true;
                if (rewrite is IComp comp)
                {
                    newFunctions.AddRange(comp.Functions);
                }
                else
                {
                    newFunctions.Add(rewrite);
                }
            }
            else
            {
                newFunctions.Add(function);
            }
        }
        
        return Optional.Of<IPointFree<IFunction<TIn, TOut>>>(rewritten
            ? new Comp<TIn, TOut>(newFunctions.ToArray(), Type)
            : this);
    }

    public override IOptional<IPointFree<IFunction<TIn, TOut>>> One(IPointFreeRule rule)
    {
        for (var i = 0; i < Functions.Length; i++)
        {
            var function = Functions[i];
            var rewrite = rule.Rewrite(function);
            if (rewrite.IsEmpty) continue;

            var value = rewrite.Value;
            if (value is IComp comp)
            {
                var newFunctions = new IPointFree<IFunction>[Functions.Length - 1 + comp.Functions.Length];
                Array.Copy(Functions, 0, newFunctions, 0, i);
                Array.Copy(comp.Functions, 0, newFunctions, i, comp.Functions.Length);
                Array.Copy(Functions, i + 1, newFunctions, i + comp.Functions.Length, Functions.Length - i - 1);
                return Optional.Of(new Comp<TIn, TOut>(newFunctions, Type));
            }
            else
            {
                var newFunctions = Functions.ToArray();
                newFunctions[i] = value;
                return Optional.Of(new Comp<TIn, TOut>(newFunctions, Type));
            }
        }
        
        return Optional.Empty<IPointFree<IFunction<TIn, TOut>>>();
    }
}