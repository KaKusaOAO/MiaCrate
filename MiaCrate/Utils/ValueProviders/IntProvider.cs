using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate;

public abstract class IntProvider
{
    private static ICodec<IEither<int, IntProvider>> ConstantOrDispatchCodec { get; } =
        Data.Codec.Either(Data.Codec.Int,
            BuiltinRegistries.IntProviderType.ByNameCodec.Dispatch(p => p.Type, t => t.Codec));

    public static ICodec<IntProvider> Codec { get; } = ConstantOrDispatchCodec.CrossSelect(
        e => e.Select(ConstantInt.Create, p => p),
        p => p.Type == IIntProviderType.Constant
            ? Either.Left<int, IntProvider>(((ConstantInt) p).Value)
            : Either.Right<int, IntProvider>(p));

    public abstract IIntProviderType Type { get; }
    public abstract int MinValue { get; }
    public abstract int MaxValue { get; }
    public abstract int Sample(IRandomSource source);

    public static ICodec<IntProvider> CreateCodec(int min, int max) => CreateCodec(min, max, Codec);
    
    public static ICodec<T> CreateCodec<T>(int min, int max, ICodec<T> codec) where T : IntProvider
    {
        return ExtraCodecs.Validate(codec, p =>
        {
            if (p.MinValue < min)
                return DataResult.Error<T>(() => $"Value provider too low: {min} [{p.MinValue}-{p.MaxValue}]");

            if (p.MaxValue > max)
                return DataResult.Error<T>(() => $"Value provider too high: {max} [{p.MinValue}-{p.MaxValue}]");

            return DataResult.Success(p);
        });
    }
}