namespace MiaCrate.Auth;

public class MinecraftProfileTexture
{
    public string Url { get; }
    private readonly Dictionary<string, string>? _metadata;
    
    public enum TextureType
    {
        Skin, Cape, Elytra
    }

    public static readonly int ProfileTextureCount = Enum.GetValues<TextureType>().Length;

    public MinecraftProfileTexture(string url, Dictionary<string, string>? metadata)
    {
        Url = url;
        _metadata = metadata;
    }

    public string? GetMetadata(string key)
    {
        if (_metadata == null) return null;
        return _metadata.TryGetValue(key, out var result) ? result : null;
    }
}