using MiaCrate.Data;

namespace MiaCrate;

public class ConstantFloat : FloatProvider
{
    public static ConstantFloat Zero { get; } = new(0);
    public new static ICodec<ConstantFloat> Codec { get; } =
        ExtraCodecs.WithAlternative(Data.Codec.Float, Data.Codec.Float.FieldOf("value").Codec)
            .CrossSelect(Create, c => c.Value);

    public override IFloatProviderType Type => IFloatProviderType.Constant;

    public override float MinValue => Value;

    public override float MaxValue => Value + 1;
    public float Value { get; }

    private ConstantFloat(float value)
    {
        Value = value;
    }

    public static ConstantFloat Create(float val) => val == 0 ? Zero : new(val);

    public override float Sample(IRandomSource source) => Value;
}