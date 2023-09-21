using MiaCrate.Data.Codecs;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Data;

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
    public static ICodec<IDynamic> Passthrough { get; } = new PassthroughCodec();

    public static ICodec<List<T>> ListOf<T>(ICodec<T> elementCodec) => new ListCodec<T>(elementCodec);
    
    public static ICodec<IEither<TFirst, TSecond>>
        Either<TFirst, TSecond>(ICodec<TFirst> first, ICodec<TSecond> second) =>
        new EitherCodec<TFirst, TSecond>(first, second);

    public static ICodec<T> Of<T>(IEncoder<T> encoder, IDecoder<T> decoder, string? name = null)
    {
        name ??= $"Codec[{encoder} {decoder}]";
        return new CompositionCodec<T>(encoder, decoder, name);
    }

    public static IMapCodec<T> Of<T>(IMapEncoder<T> encoder, IMapDecoder<T> decoder, Func<string> name) => 
        MapCodec.Of(encoder, decoder, name);

    public static IMapCodec<IOptional<T>> OptionalField<T>(string name, ICodec<T> elementCodec) =>
        new OptionalFieldCodec<T>(name, elementCodec);
    
    public static IMapCodec<IPair<TFirst, TSecond>> MapPair<TFirst, TSecond>
        (IMapCodec<TFirst> first, IMapCodec<TSecond> second) =>
        new PairMapCodec<TFirst, TSecond>(first, second);

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

    private class PassthroughCodec : ICodec<IDynamic>
    {
        public IDataResult<TDynamic> Encode<TDynamic>(IDynamic input, IDynamicOps<TDynamic> ops, TDynamic prefix)
        {
            if (input.Value == input.Ops.Empty)
            {
                // Nothing to merge, return rest.
                return DataResult.Success(prefix, Lifecycle.Experimental);
            }

            var casted = input.Convert(ops).Value;
            if (prefix?.Equals(ops.Empty) ?? true)
            {
                // No need to merge anything, return the old value.
                return DataResult.Success(casted, Lifecycle.Experimental);
            }

            throw new NotImplementedException();
        }

        public IDataResult<IPair<IDynamic, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
        {
            IDynamic dyn = new Dynamic<TIn>(ops, input);
            return DataResult.Success(Pair.Of(dyn, ops.Empty));
        }
    }
}