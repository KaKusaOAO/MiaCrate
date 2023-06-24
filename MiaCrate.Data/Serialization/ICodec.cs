using Mochi.Core;

namespace MiaCrate.Data.Codecs;

public interface ICodec : IEncoder, IDecoder
{
    
}

public interface ICodec<T> : ICodec, IEncoder<T>, IDecoder<T>
{
    public ICodec<TOut> CrossSelect<TOut>(Func<T, TOut> to, Func<TOut, T> from) => Codec.Of(
        CoSelect(from), Select(to),
        $"{this}[xmapped]"
    );
    
    public new IMapCodec<T> FieldOf(string name) => MapCodec.Of(
        new FieldEncoder<T>(name, this),
        new FieldDecoder<T>(name, this),
        () => $"Field[{name}: {this}]"
    );
    IMapEncoder<T> IEncoder<T>.FieldOf(string name) => FieldOf(name);
    IMapDecoder<T> IDecoder<T>.FieldOf(string name) => FieldOf(name);
}

public static class Codec
{
    public static IMapCodec<Unit> Empty { get; } = MapCodec.Of(Encoder.Empty<Unit>(), Decoder.Unit(Unit.Instance));
    public static IPrimitiveCodec<bool> Bool { get; } = new BoolPrimitiveCodec();
    public static IPrimitiveCodec<byte> Byte { get; } = new BytePrimitiveCodec();
    
    public static ICodec<T> Of<T>(IEncoder<T> encoder, IDecoder<T> decoder, string? name = null)
    {
        name ??= $"Codec[{encoder} {decoder}]";
        return new CompositionCodec<T>(encoder, decoder, name);
    }
    
    private class CompositionCodec<T> : ICodec<T>
    {
        private readonly IEncoder<T> _encoder;
        private readonly IDecoder<T> _decoder;
        private readonly string _name;

        public CompositionCodec(IEncoder<T> encoder, IDecoder<T> decoder, string name)
        {
            _encoder = encoder;
            _decoder = decoder;
            _name = name;
        }

        public IDataResult<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, TOut prefix) => 
            _encoder.Encode(input, ops, prefix);

        public IDataResult<IPair<T, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input) =>
            _decoder.Decode(ops, input);

        public override string ToString() => _name;
    }

    private class BoolPrimitiveCodec : IPrimitiveCodec<bool>
    {
        public IDataResult<bool> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetBoolValue(input);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, bool value) => ops.CreateBool(value);
    }
    
    private class BytePrimitiveCodec : IPrimitiveCodec<byte>
    {
        public IDataResult<byte> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetNumberValue(input).Select(d => (byte)d);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, byte value) => ops.CreateByte(value);
    }
}