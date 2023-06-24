namespace MiaCrate.Data.Codecs;

public interface IRecordCodecBuilder : IApp
{
    public interface IMu : IK1
    {
        
    }
}

public interface IRecordCodecBuilderLeft<T> : IRecordCodecBuilder, IAppLeft<IRecordCodecBuilderLeft<T>.IMu>
{
    public interface IMu : IRecordCodecBuilder.IMu
    {
        
    }
}

public interface IRecordCodecBuilderRight<T> : IRecordCodecBuilder, IAppRight<T>
{
    
}

public interface IRecordCodecBuilder<TLeft, TRight> : IApp<IRecordCodecBuilderLeft<TLeft>.IMu, TRight>,
    IRecordCodecBuilderLeft<TLeft>, IRecordCodecBuilderRight<TRight>
{
}

public class RecordCodecBuilder<TLeft, TRight> : IRecordCodecBuilder<TLeft, TRight>
{
    internal Func<TLeft, TRight> Getter { get; }
    internal Func<TLeft, IMapEncoder<TRight>> Encoder { get; }
    internal IMapDecoder<TRight> Decoder { get; }

    internal RecordCodecBuilder(Func<TLeft, TRight> getter, Func<TLeft, IMapEncoder<TRight>> encoder, IMapDecoder<TRight> decoder)
    {
        Getter = getter;
        Encoder = encoder;
        Decoder = decoder;
    }
} 

public static class RecordCodecBuilder
{
    public static RecordCodecBuilder<TLeft, TRight> Unbox<TLeft, TRight>(
        IApp<IRecordCodecBuilderLeft<TLeft>.IMu, TRight> box) => (RecordCodecBuilder<TLeft, TRight>)box;

    public static ICodec<T> Create<T>(Func<Instance<T>, IApp<IRecordCodecBuilderLeft<T>.IMu, T>> builder) =>
        MapCodec(builder).Codec;
    
    public static IMapCodec<T> MapCodec<T>(Func<Instance<T>, IApp<IRecordCodecBuilderLeft<T>.IMu, T>> builder) => 
        Build(builder(CreateInstance<T>()));

    public static RecordCodecBuilder<TLeft, TRight> Point<TLeft, TRight>(TRight instance) =>
        new(
            _ => instance,
            _ => Encoder.Empty<TRight>(),
            Decoder.Unit(instance));

    public static IMapCodec<T> Build<T>(IApp<IRecordCodecBuilderLeft<T>.IMu, T> builderBox)
    {
        var builder = Unbox(builderBox);
        return new BuiltMapCodec<T>(builder);
    }

    private class BuiltMapCodec<T> : MapCodec<T>
    {
        private readonly RecordCodecBuilder<T, T> _builder;

        public BuiltMapCodec(RecordCodecBuilder<T, T> builder)
        {
            _builder = builder;
        }

        public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => _builder.Decoder.GetKeys(ops);

        public override IRecordBuilder<TOut> Encode<TOut>(T input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
            _builder.Encoder(input).Encode(input, ops, prefix);

        public override IDataResult<T> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) => 
            _builder.Decoder.Decode(ops, input);

        public override string ToString() => $"RecordCodec[{_builder.Decoder}]";
    }

    public static Instance<T> CreateInstance<T>() => new();

    public class Instance<T> : IApplicative<IRecordCodecBuilderLeft<T>.IMu, Instance<T>.Mu>
    {
        public class Mu : IApplicative.IMu
        {
            
        }

        public IApp<IRecordCodecBuilderLeft<T>.IMu, TResult> Map<TArg, TResult>(Func<TArg, TResult> func, IApp<IRecordCodecBuilderLeft<T>.IMu, TArg> ts)
        {
            var unbox = Unbox(ts);
            var getter = unbox.Getter;

            return new RecordCodecBuilder<T, TResult>(
                o => func(getter(o)),
                o => new MappedMapEncoder<TArg, TResult>(unbox, getter, o),
                unbox.Decoder.Select(func));
        }

        private class MappedMapEncoder<TArg, TResult> : MapEncoder.Implementation<TResult>
        {
            private readonly Func<T, TArg> _getter;
            private readonly T _o;
            private readonly IMapEncoder<TArg> _encoder;

            public MappedMapEncoder(RecordCodecBuilder<T, TArg> unbox, Func<T, TArg> getter, T o)
            {
                _getter = getter;
                _o = o;
                _encoder = unbox.Encoder(o);
            }


            public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => _encoder.GetKeys(ops);

            public override IRecordBuilder<TOut> Encode<TOut>(TResult input, IDynamicOps<TOut> ops, IRecordBuilder<TOut> prefix) => 
                _encoder.Encode(_getter(_o), ops, prefix);
        }

        public IApp<IRecordCodecBuilderLeft<T>.IMu, TValue> Point<TValue>(TValue value) =>
            RecordCodecBuilder.Point<T, TValue>(value);

        public Func<IApp<IRecordCodecBuilderLeft<T>.IMu, TArg>, IApp<IRecordCodecBuilderLeft<T>.IMu, TResult>> Lift1<TArg, TResult>(IApp<IRecordCodecBuilderLeft<T>.IMu, Func<TArg, TResult>> func)
        {
            return fa =>
            {
                var f = Unbox(func);
                var a = Unbox(fa);

                return new RecordCodecBuilder<T, TResult>(
                    o => f.Getter(o)(a.Getter(o)),
                    o =>
                    {
                        var fEnc = f.Encoder(o);
                        var aEnc = a.Encoder(o);
                        var aFromO = a.Getter(o);
                        return new InstanceEncoder<TArg, TResult>(aEnc, aFromO, fEnc);
                    },
                    new InstanceDecoder<TArg, TResult>(f, a));
            };
        }

        private class InstanceEncoder<TArg, TResult> : MapEncoder.Implementation<TResult>
        {
            private readonly IMapEncoder<TArg> _aEnc;
            private readonly TArg _aFromO;
            private readonly IMapEncoder<Func<TArg, TResult>> _fEnc;

            public InstanceEncoder(IMapEncoder<TArg> aEnc, TArg aFromO, IMapEncoder<Func<TArg, TResult>> fEnc)
            {
                _aEnc = aEnc;
                _aFromO = aFromO;
                _fEnc = fEnc;
            }

            public override IEnumerable<TDynamic> GetKeys<TDynamic>(IDynamicOps<TDynamic> ops) => 
                _aEnc.GetKeys(ops).Concat(_fEnc.GetKeys(ops));

            public override IRecordBuilder<TDynamic> Encode<TDynamic>(TResult input, IDynamicOps<TDynamic> ops, IRecordBuilder<TDynamic> prefix)
            {
                _aEnc.Encode(_aFromO, ops, prefix);
                _fEnc.Encode(_ => input, ops, prefix);
                return prefix;
            }

            public override string ToString() => $"{_fEnc} * {_aEnc}";
        }

        private class InstanceDecoder<TArg, TResult> : MapDecoder.Implementation<TResult>
        {
            private readonly RecordCodecBuilder<T, Func<TArg, TResult>> _f;
            private readonly RecordCodecBuilder<T, TArg> _a;

            public InstanceDecoder(RecordCodecBuilder<T, Func<TArg, TResult>> f, RecordCodecBuilder<T, TArg> a)
            {
                _f = f;
                _a = a;
            }

            public override IEnumerable<T1> GetKeys<T1>(IDynamicOps<T1> ops) => 
                _a.Decoder.GetKeys(ops).Concat(_f.Decoder.GetKeys(ops));

            public override IDataResult<TResult> Decode<TIn>(IDynamicOps<TIn> ops, IMapLike<TIn> input) =>
                _a.Decoder.Decode(ops, input).SelectMany(ar =>
                    _f.Decoder.Decode(ops, input).Select(fr => fr(ar)));
        }
    }

    public static IRecordCodecBuilder<TLeft, TRight> Of<TLeft, TRight>(Func<TLeft, TRight> getter, IMapCodec<TRight> codec) => 
        new RecordCodecBuilder<TLeft, TRight>(getter, _ => codec, codec);
}