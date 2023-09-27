using MiaCrate.Client.Audio;
using MiaCrate.Client.Resources;
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
    public const string MissingSound = "FOR THE DEBUG!";
    public const string OpenAlSoftPrefix = "OpenAL Soft on ";
    
    public static readonly int OpenAlSoftPrefixLength = OpenAlSoftPrefix.Length;
    private readonly SoundManager _soundManager;
    private readonly Options _options;
    private bool _loaded;
    private readonly Library _library = new();
    private readonly Listener _listener;
    private readonly List<Sound> _preloadQueue = new();

    public SoundEngine(SoundManager soundManager, Options options, IResourceProvider provider)
    {
        _soundManager = soundManager;
        _options = options;
        _listener = _library.Listener;
    }

    public void Play(ISoundInstance soundInstance)
    {
        Util.LogFoobar();
        if (!_loaded) return;
    }

    public void RequestPreload(Sound sound)
    {
        _preloadQueue.Add(sound);
    }
}