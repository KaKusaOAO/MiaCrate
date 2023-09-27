namespace MiaCrate.Sounds;

public enum SoundSource
{
    Master,
    Music,
    Record,
    Weather,
    Block,
    Hostile,
    Neutral,
    Player,
    Ambient,
    Voice
}

public static class SoundSourceExtension
{
    public static string GetName(this SoundSource source) => Enum.GetName(source)!.ToLowerInvariant();
}