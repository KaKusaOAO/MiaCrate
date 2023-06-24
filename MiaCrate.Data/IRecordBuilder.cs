using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IRecordBuilder
{
    
}

public interface IRecordBuilder<T> : IRecordBuilder
{
    IDynamicOps<T> Ops { get; }

    IRecordBuilder<T> Add(T key, T value);
    IRecordBuilder<T> Add(T key, IDataResult<T> value);
    IRecordBuilder<T> Add(IDataResult<T> key, IDataResult<T> value);
    IRecordBuilder<T> WithErrorsFrom(IDataResult result);
    IRecordBuilder<T> SetLifecycle(Lifecycle lifecycle);
    IRecordBuilder<T> SelectError(Func<string, string> onError);
    IDataResult<T> Build(T prefix);

    IDataResult<T> Build(IDataResult<T> prefix) => prefix.SelectMany(Build);
    IRecordBuilder<T> Add(string key, T value) => Add(Ops.CreateString(key), value);
    IRecordBuilder<T> Add(string key, IDataResult<T> value) => Add(Ops.CreateString(key), value);

    IRecordBuilder<T> Add<TIn>(string key, TIn value, IEncoder<TIn> encoder) =>
        Add(key, encoder.EncodeStart(Ops, value));
}

public abstract class AbstractBuilder<TOps, TBuilder> : IRecordBuilder<TOps>
{
    protected IDataResult<TBuilder> Builder { get; set; }

    protected AbstractBuilder(IDynamicOps<TOps> ops, Func<TBuilder>? builder = null)
    {
        Ops = ops;
        Builder = DataResult.Success((builder ?? InitBuilder)(), Lifecycle.Stable);
    }

    public IDynamicOps<TOps> Ops { get; }

    public abstract IRecordBuilder<TOps> Add(TOps key, TOps value);
    public abstract IRecordBuilder<TOps> Add(TOps key, IDataResult<TOps> value);
    public abstract IRecordBuilder<TOps> Add(IDataResult<TOps> key, IDataResult<TOps> value);
    protected abstract IDataResult<TOps> Build(TBuilder builder, TOps prefix);
    protected abstract TBuilder InitBuilder();

    public IRecordBuilder<TOps> WithErrorsFrom(IDataResult result)
    {
        Builder = Builder.SelectMany(v => result.Select(r => v));
        return this;
    }

    public IRecordBuilder<TOps> SetLifecycle(Lifecycle lifecycle)
    {
        Builder = Builder.SetLifecycle(lifecycle);
        return this;
    }

    public IRecordBuilder<TOps> SelectError(Func<string, string> onError)
    {
        Builder = Builder.SelectError(onError);
        return this;
    }

    public IDataResult<TOps> Build(TOps prefix)
    {
        var result = Builder.SelectMany(b => Build(b, prefix));
        Builder = DataResult.Success(InitBuilder(), Lifecycle.Stable);
        return result;
    }
}

public abstract class AbstractStringBuilder<TOps, TBuilder> : AbstractBuilder<TOps, TBuilder>, IRecordBuilder<TOps>
{
    protected AbstractStringBuilder(IDynamicOps<TOps> ops, Func<TBuilder>? builder = null) : base(ops, builder) { }

    protected abstract TBuilder Append(string key, TOps value, TBuilder builder);
    
    public IRecordBuilder<TOps> Add(string key, TOps value)
    {
        Builder = Builder.Select(b => Append(key, value, b));
        return this;
    }

    public IRecordBuilder<TOps> Add(string key, IDataResult<TOps> value)
    {
        Builder = Builder.Apply2Stable((b, v) => Append(key, v, b), value);
        return this;
    }

    public override IRecordBuilder<TOps> Add(TOps key, TOps value)
    {
        Builder = Ops.GetStringValue(key).SelectMany(k =>
        {
            Add(k, value);
            return Builder;
        });

        return this;
    }

    public override IRecordBuilder<TOps> Add(TOps key, IDataResult<TOps> value)
    {
        Builder = Ops.GetStringValue(key).SelectMany(k =>
        {
            Add(k, value);
            return Builder;
        });

        return this;
    }

    public override IRecordBuilder<TOps> Add(IDataResult<TOps> key, IDataResult<TOps> value)
    {
        Builder = key.SelectMany(k => Ops.GetStringValue(k)).SelectMany(k =>
        {
            Add(k, value);
            return Builder;
        });

        return this;
    }
}

public abstract class AbstractUniversalBuilder<TOps, TBuilder> : AbstractBuilder<TOps, TBuilder>
{
    protected AbstractUniversalBuilder(IDynamicOps<TOps> ops, Func<TBuilder>? builder = null) : base(ops, builder) { }
    
    protected abstract TBuilder Append(TOps key, TOps value, TBuilder builder);

    public override IRecordBuilder<TOps> Add(TOps key, TOps value)
    {
        Builder = Builder.Select(b => Append(key, value, b));
        return this;
    }

    public override IRecordBuilder<TOps> Add(TOps key, IDataResult<TOps> value)
    {
        Builder = Builder.Apply2Stable((b, v) => Append(key, v, b), value);
        return this;
    }

    public override IRecordBuilder<TOps> Add(IDataResult<TOps> key, IDataResult<TOps> value)
    {
        Builder = Builder.Ap(
            key.Apply2Stable<TOps, Func<TBuilder, TBuilder>>(
                (k, v) => b => Append(k, v, b), value));
        return this;
    }
}

internal sealed class DictionaryBuilder<T> : AbstractUniversalBuilder<T, Dictionary<T, T>> where T : notnull
{
    public DictionaryBuilder(IDynamicOps<T> ops, Func<Dictionary<T, T>>? builder = null) : base(ops, builder)
    {
    }

    protected override IDataResult<T> Build(Dictionary<T, T> builder, T prefix) => Ops.MergeToMap(prefix, builder);

    protected override Dictionary<T, T> InitBuilder() => new();

    protected override Dictionary<T, T> Append(T key, T value, Dictionary<T, T> builder)
    {
        builder[key] = value;
        return builder;
    }
}