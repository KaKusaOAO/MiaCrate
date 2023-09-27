using Mochi.Texts;

namespace MiaCrate.Texts;

public static class MiaContentTypes
{
    public static LiteralContentType Literal => TextContentTypes.Literal;
    
    public static TranslatableContentType Translatable { get; } = 
        TextContentTypes.Register(TranslatableContentType.Key, new TranslatableContentType());

    public static void Bootstrap() {}
}