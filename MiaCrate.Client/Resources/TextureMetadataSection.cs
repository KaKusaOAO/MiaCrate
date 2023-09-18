namespace MiaCrate.Client.Resources;

public class TextureMetadataSection
{
    public static readonly TextureMetadataSectionSerializer Serializer = new();
    
    public bool IsBlur { get; }
    public bool IsClamp { get; }

    public TextureMetadataSection(bool isBlur, bool isClamp)
    {
        IsBlur = isBlur;
        IsClamp = isClamp;
    }
}