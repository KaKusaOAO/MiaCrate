using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public interface ICodec : IEncoder, IDecoder
{
    
}

public interface ICodec<T> : ICodec, IEncoder<T>, IDecoder<T>
{
    public ICodec<TOut> CrossSelect<TOut>(Func<T, TOut> to, Func<TOut, T> from) => Codec.Of(
        CoSelect(from), Select(to),
        $"{this}[xmapped]"
    );
    
    public ICodec<TOut> CoSelectSelectMany<TOut>(Func<T, IDataResult<TOut>> to, Func<TOut, T> from) => Codec.Of(
        CoSelect(from), SelectMany(to), 
        $"{this}[comapFlatMapped]"
    );
    
    public ICodec<TOut> FlatCoSelectSelect<TOut>(Func<T, TOut> to, Func<TOut, IDataResult<T>> from) => Codec.Of(
        FlatCoSelect(from), Select(to), 
        $"{this}[flatComapMapped]"
    );

    public ICodec<TOut> FlatCrossSelect<TOut>(Func<T, IDataResult<TOut>> to, Func<TOut, IDataResult<T>> from) =>
        Codec.Of(FlatCoSelect(from), SelectMany(to), $"{this}[flatXmapped]");

    public ICodec<TOut> Cast<TOut>() =>
        Codec.Of(
            CoSelect((TOut t) => (T)(object)t!), 
            Select(t => (TOut)(object)t!), $"{this}[casted]");

    public ICodec<TOut> Dispatch<TOut>(Func<TOut, T> type, Func<T, ICodec<TOut>> codec) =>
        Dispatch("type", type, codec);
    
    public ICodec<TOut> Dispatch<TOut>(string typeKey, Func<TOut, T> type, Func<T, ICodec<TOut>> codec) => 
        PartialDispatch(typeKey, 
            v => DataResult.Success(type(v)), 
            v => DataResult.Success(codec(v)));

    public ICodec<TOut> PartialDispatch<TOut>(string typeKey,
        Func<TOut, IDataResult<T>> type,
        Func<T, IDataResult<ICodec<TOut>>> codec) =>
        new KeyDispatchCodec<T, TOut>(typeKey, this, type, codec).Codec;

    public ICodec<T> WithLifecycle(Lifecycle lifecycle) => new LifecycleCodec(this, lifecycle);

    public ICodec<T> Stable => WithLifecycle(Lifecycle.Stable);
    
    public new IMapCodec<T> FieldOf(string name) => MapCodec.Of(
        new FieldEncoder<T>(name, this),
        new FieldDecoder<T>(name, this),
        () => $"Field[{name}: {this}]"
    );
    IMapEncoder<T> IEncoder<T>.FieldOf(string name) => FieldOf(name);
    IMapDecoder<T> IDecoder<T>.FieldOf(string name) => FieldOf(name);

    public IMapCodec<IOptional<T>> OptionalFieldOf(string name) => Codec.OptionalField(name, this);

    public IMapCodec<T> OptionalFieldOf(string name, T defaultValue)
    {
        return Codec.OptionalField(name, this).CrossSelect(
            o => o.OrElse(defaultValue),
            a => a!.Equals(defaultValue) ? Optional.Empty<T>() : Optional.Of<T>(a)
        );
    }

    public ICodec<List<T>> ListCodec => Codec.ListOf(this);

    public ICodec<T> SelectResult(IResultFunction function) => new ResultMappedCodec(this, function);

    private class ResultMappedCodec : ICodec<T>
    {
        private readonly ICodec<T> _inner;
        private readonly IResultFunction _func;

        public ResultMappedCodec(ICodec<T> inner, IResultFunction func)
        {
            _inner = inner;
            _func = func;
        }
        
        public IDataResult<TDynamic> Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix) => 
            _func.CoApply(ops, input, _inner.Encode(input, ops, prefix));

        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
            _func.Apply(ops, input, _inner.Decode(ops, input));

        public override string ToString() => $"{_inner}[mapResult {_func}]";
    }

    private class LifecycleCodec : ICodec<T>
    {
        private readonly ICodec<T> _inner;
        private readonly Lifecycle _lifecycle;

        public LifecycleCodec(ICodec<T> inner, Lifecycle lifecycle)
        {
            _inner = inner;
            _lifecycle = lifecycle;
        }

        public IDataResult<TDynamic> Encode<TDynamic>(T input, IDynamicOps<TDynamic> ops, TDynamic prefix) =>
            _inner.Encode(input, ops, prefix).SetLifecycle(_lifecycle);

        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
            _inner.Decode(ops, input).SetLifecycle(_lifecycle);

        public override string ToString() => _inner.ToString()!;
    }
    
    public interface IResultFunction
    {
        public IDataResult<IPair<T, TOps>>
            Apply<TOps>(IDynamicOps<TOps> ops, TOps input, IDataResult<IPair<T, TOps>> a);
        
        public IDataResult<TOps>
            CoApply<TOps>(IDynamicOps<TOps> ops, T input, IDataResult<TOps> t);
    }
}

public static class CodecExtension
{
    public static IMapCodec<T> FieldOf<T>(this ICodec<T> codec, string name) => codec.FieldOf(name);
}