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
            throw new NotImplementedException();
        }

        public IApp<IRecordCodecBuilderLeft<T>.IMu, TValue> Point<TValue>(TValue value) =>
            RecordCodecBuilder.Point<T, TValue>(value);

        public Func<IApp<IRecordCodecBuilderLeft<T>.IMu, TArg>, IApp<IRecordCodecBuilderLeft<T>.IMu, TResult>> Lift1<TArg, TResult>(IApp<IRecordCodecBuilderLeft<T>.IMu, Func<TArg, TResult>> func)
        {
            throw new NotImplementedException();
        }
    }

    public static IRecordCodecBuilder<TLeft, TRight> Of<TLeft, TRight>(Func<TLeft, TRight> getter, IMapCodec<TRight> codec) => 
        new RecordCodecBuilder<TLeft, TRight>(getter, _ => codec, codec);
}