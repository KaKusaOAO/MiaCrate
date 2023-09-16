using MiaCrate.Resources;

namespace MiaCrate.Client.Sounds;

public class SoundEngine
{
    private const float MinPitch = 0.5f;
    private const float MaxPitch = 2f;
    private const float MinVolume = 0f;
    private const float MaxVolume = 1f;
    private const int MinSourceLifetime = 20;
    private const long DefaultDeviceCheckIntervalMs = 1000L;
    public const string OpenAlSoftPrefix = "OpenAL Soft on ";
    
    public static readonly int OpenAlSoftPrefixLength = OpenAlSoftPrefix.Length;
    
    public SoundEngine(SoundManager manager, Options options, IResourceProvider provider)
    {
        
    }
}