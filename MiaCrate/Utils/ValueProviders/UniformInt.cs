using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate;

public class UniformInt : IntProvider
{
    public new static ICodec<UniformInt> Codec { get; } =
        RecordCodecBuilder.Create<UniformInt>(instance => instance
            .Group(
                Data.Codec.Int.FieldOf("min_inclusive").ForGetter<UniformInt>(u => u.MinValue),
                Data.Codec.Int.FieldOf("max_inclusive").ForGetter<UniformInt>(u => u.MaxValue)
            )
            .Apply(instance, (a, b) => new UniformInt(a, b))
        );

    public override IIntProviderType Type => IIntProviderType.Uniform;

    public override int MinValue { get; }

    public override int MaxValue { get; }

    private UniformInt(int min, int max)
    {
        MinValue = min;
        MaxValue = max;
    }

    public static UniformInt Of(int min, int max) => new(min, max);
    
    public override int Sample(IRandomSource source) => 
        Util.RandomBetweenInclusive(source, MinValue, MinValue);
}