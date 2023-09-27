using MiaCrate.Sounds;

namespace MiaCrate.Client.Resources;

public abstract class AbstractSoundInstance : ISoundInstance
{
    public ResourceLocation Location { get; }
    public SoundSource Source { get; }
    public bool IsLooping { get; protected set; }
    public bool IsRelative { get; protected set; }
    public int Delay { get; protected set; }
    public float Volume { get; protected set; }
    public float Pitch { get; protected set; }
    public double X { get; protected set; }
    public double Y { get; protected set; }
    public double Z { get; protected set; }

    public AttenuationType Attenuation { get; protected set; }

    private readonly IRandomSource _random;

    protected AbstractSoundInstance(ResourceLocation location, SoundSource source, IRandomSource random)
    {
        Volume = 1;
        Pitch = 1;
        Attenuation = AttenuationType.Linear;
        Location = location;
        Source = source;
        _random = random;
    }
    
    protected AbstractSoundInstance(SoundEvent soundEvent, SoundSource source, IRandomSource random)
        : this(soundEvent.Location, source, random) {}
    
}