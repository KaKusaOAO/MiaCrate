using Mochi.Core;

namespace MiaCrate.Data;

public interface IBang : IFuncPointFreeOut<Unit> {}
public interface IBang<T> : IBang, IFuncPointFree<T, Unit> {}

public class Bang<T> : PointFree<IFunction<T, Unit>>, IBang<T>
{
    private IType<T> _type;
    public override IType<IFunction<T, Unit>> Type => Dsl.Func(_type, Dsl.EmptyPartType);

    public Bang(IType<T> type)
    {
        _type = type;
    }

    public override string ToString(int level) => "!";

    public override Func<IDynamicOps, IFunction<T, Unit>> Eval() => 
        _ => Function.Create<T, Unit>(_ => Unit.Instance);
}