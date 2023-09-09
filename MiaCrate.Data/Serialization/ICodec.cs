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

    public ICodec<TOut> CrossSelectMany<TOut>(Func<T, IDataResult<TOut>> to, Func<TOut, IDataResult<T>> from) =>
        Codec.Of(CoSelectMany(from), SelectMany(to), $"{this}[flatXmapped]");

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
}