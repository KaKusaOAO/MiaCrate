using MiaCrate.Sounds;

namespace MiaCrate.Client.Resources;

public interface ISoundInstance
{
    public ResourceLocation Location { get; }
    public SoundSource Source { get; }
    public bool IsLooping { get; }
    public bool IsRelative { get; }
    public int Delay { get; }
    public float Volume { get; }
    public float Pitch { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public AttenuationType Attenuation { get; }

    public bool CanStartSilent => false;
    public bool CanPlaySound => true;

    public static IRandomSource CreateUnseededRandom() => IRandomSource.Create();
}