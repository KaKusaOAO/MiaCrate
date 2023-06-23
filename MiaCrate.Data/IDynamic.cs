using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IDynamic : IDynamicLike
{
    
}

public interface IDynamic<T> : IDynamic, IDynamicLike<T>
{
    public T Value { get; }
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
}