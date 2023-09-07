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
    public static IPrimitiveCodec<int> Int { get; } = new IntPrimitiveCodec();
    public static IPrimitiveCodec<long> Long { get; } = new LongPrimitiveCodec();
    public static IPrimitiveCodec<float> Float { get; } = new FloatPrimitiveCodec();
    public static IPrimitiveCodec<double> Double { get; } = new DoublePrimitiveCodec();
    public static IPrimitiveCodec<string> String { get; } = new StringPrimitiveCodec();

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
    
    private class IntPrimitiveCodec : IPrimitiveCodec<int>
    {
        public IDataResult<int> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetNumberValue(input).Select(e => (int) e);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, int value) => ops.CreateInt(value);
    }
    
    private class LongPrimitiveCodec : IPrimitiveCodec<long>
    {
        public IDataResult<long> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetNumberValue(input).Select(d => (long) d);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, long value) => ops.CreateLong(value);
    }
    
    private class FloatPrimitiveCodec : IPrimitiveCodec<float>
    {
        public IDataResult<float> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetNumberValue(input).Select(d => (float) d);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, float value) => ops.CreateFloat(value);
    }
    
    private class DoublePrimitiveCodec : IPrimitiveCodec<double>
    {
        public IDataResult<double> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetNumberValue(input).Select(d => (double) d);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, double value) => ops.CreateDouble(value);
    }
    
    private class StringPrimitiveCodec : IPrimitiveCodec<string>
    {
        public IDataResult<string> Read<TIn>(IDynamicOps<TIn> ops, TIn input) => ops.GetStringValue(input);
        public TOut Write<TOut>(IDynamicOps<TOut> ops, string value) => ops.CreateString(value);
    }
}