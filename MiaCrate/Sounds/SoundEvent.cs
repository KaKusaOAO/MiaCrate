using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Extensions;
using Mochi.IO;
using Mochi.Utils;

namespace MiaCrate.Sounds;

public class SoundEvent
{
    public static ICodec<SoundEvent> DirectCodec { get; } =
        RecordCodecBuilder.Create<SoundEvent>(instance => instance
            .Group(
                ResourceLocation.Codec.FieldOf("sound_id").ForGetter<SoundEvent>(s => s.Location),
                Data.Codec.Float.OptionalFieldOf("range").ForGetter<SoundEvent>(s => s.FixedRange)
            )
            .Apply(instance, Create)
        );

    private const float DefaultRange = 16;

    private readonly float _range;
    private readonly bool _newSystem;

    public ResourceLocation Location { get; }
    private IOptional<float> FixedRange => _newSystem ? Optional.Of(_range) : Optional.Empty<float>();

    private SoundEvent(ResourceLocation location, float range, bool newSystem)
    {
        Location = location;
        _range = range;
        _newSystem = newSystem;
    }

    private static SoundEvent Create(ResourceLocation location, IOptional<float> optional)
    {
        return optional.Select(f => CreateFixedRangeEvent(location, f))
            .OrElseGet(() => CreateVariableRangeEvent(location));
    }

    public static SoundEvent CreateVariableRangeEvent(ResourceLocation location) => 
        new(location, DefaultRange, false);

    public static SoundEvent CreateFixedRangeEvent(ResourceLocation location, float range) => 
        new(location, range, true);

    public float GetRange(float f)
    {
        if (_newSystem) return _range;
        return f > 1 ? DefaultRange * f : DefaultRange;
    }

    public void WriteToNetwork(BufferWriter writer)
    {
        writer.WriteResourceLocation(Location);
        writer.WriteOptional(FixedRange, (w, f) => w.WriteFloat(f));
    }

    public static SoundEvent ReadFromNetwork(BufferReader reader)
    {
        var location = reader.ReadResourceLocation();
        var optional = reader.ReadOptional(w => w.ReadSingle());
        return Create(location, optional);
    }
}