using System.Runtime.CompilerServices;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IDynamic : IDynamicLike
{
    public object? Value { get; }
    public IDynamic<T> Convert<T>(IDynamicOps<T> outOps);

    public static T Convert<T>(IDynamicOps inOps, IDynamicOps<T> outOps, object? input)
    {
        if (inOps == outOps) return (T) input!;
        return inOps.ConvertTo(outOps, input);
    }
    
    public static T Convert<TSource, T>(IDynamicOps<TSource> inOps, IDynamicOps<T> outOps, TSource input) 
        where T : class where TSource : class
    {
        if (inOps == outOps) return (input as T)!;
        return inOps.ConvertTo(outOps, input);
    }
}

public interface IDynamic<T> : IDynamic, IDynamicLike<T>
{
    public new T Value { get; }
    object? IDynamic.Value => Value;
    
    public IDynamic<T> Select(Func<T, T> func);
}

public class Dynamic<T> : DynamicLike<T>, IDynamic<T>
{
    public T Value { get; }

    public Dynamic(IDynamicOps<T> ops) : this(ops, ops.Empty) {}
    
    public Dynamic(IDynamicOps<T> ops, T? value = default) : base(ops)
    {
        Value = value ?? ops.Empty;
    }

    private IDynamic<T> Boxed() => this;
    public IDynamic<T> Select(Func<T, T> func) => new Dynamic<T>(Ops, func(Value));

    public override IDataResult<decimal> AsNumber() => Ops.GetNumberValue(Value);
    public override IDataResult<string> AsString() => Ops.GetStringValue(Value);

    public override IDataResult<IEnumerable<IDynamic<T>>> AsEnumerableOpt() =>
        Ops.GetEnumerable(Value).Select(s => s.Select(e => new Dynamic<T>(Ops, e).Boxed()));
    
    public IDynamic<TOut> Convert<TOut>(IDynamicOps<TOut> outOps) => 
        new Dynamic<TOut>(outOps, IDynamic.Convert(Ops, outOps, Value));
}