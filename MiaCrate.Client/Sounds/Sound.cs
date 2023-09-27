using MiaCrate.Resources;

namespace MiaCrate.Client.Sounds;

public class Sound : IWeighted<Sound>
{
    public static FileToIdConverter SoundLister { get; } = new("sounds", ".ogg");


    public ResourceLocation Location { get; }
    public ResourceLocation Path => SoundLister.IdToFile(Location);
    public ISampledFloat Volume { get; }
    public ISampledFloat Pitch { get; }
    public int Weight { get; }
    public SoundType Type { get; }
    public bool ShouldStream { get; }
    public bool ShouldPreload { get; }
    public int AttenuationDistance { get; }

    public Sound(string location, ISampledFloat volume, ISampledFloat pitch, int weight, SoundType type, bool stream,
        bool preload, int attenuationDistance)
    {
        Location = new ResourceLocation(location);
        Volume = volume;
        Pitch = pitch;
        Weight = weight;
        Type = type;
        ShouldStream = stream;
        ShouldPreload = preload;
        AttenuationDistance = attenuationDistance;
    }
    
    public Sound GetSound(IRandomSource random) => this;

    public void PreloadIfRequired(SoundEngine engine)
    {
        if (!ShouldPreload) return;
        engine.RequestPreload(this);
    }

    public sealed class SoundType : IEnumLike<SoundType>
    {
        private static readonly Dictionary<int, SoundType> _values = new();

        public static SoundType File { get; } = new("file");
        public static SoundType SoundEvent { get; } = new("event");

        private readonly string _name;
 
        public int Ordinal { get; }

        public static SoundType[] Values => _values.Values.ToArray();

        private SoundType(string name)
        {
            _name = name;

            Ordinal = _values.Count;
            _values[Ordinal] = this;
        }

        public static SoundType? GetByName(string name) => 
            _values.Values.FirstOrDefault(v => v._name == name);
    }
}