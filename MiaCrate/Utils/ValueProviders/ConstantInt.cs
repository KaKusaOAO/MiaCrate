using MiaCrate.Data;

namespace MiaCrate;

public class ConstantInt : IntProvider
{
    public static ConstantInt Zero { get; } = new(0);
    public new static ICodec<ConstantInt> Codec { get; } =
        ExtraCodecs.WithAlternative(Data.Codec.Int, Data.Codec.Int.FieldOf("value").Codec)
            .CrossSelect(Create, c => c.Value);

    public override IIntProviderType Type => IIntProviderType.Constant;

    public override int MinValue => Value;

    public override int MaxValue => Value;
    public int Value { get; }

    private ConstantInt(int value)
    {
        Value = value;
    }

    public static ConstantInt Create(int val) => val == 0 ? Zero : new(val);

    public override int Sample(IRandomSource source) => Value;
}