using Mochi.Texts;

namespace MiaCrate.Texts;

public static class MiaContentTypes
{
    public static LiteralContentType Literal => TextContentTypes.Literal;
    public static TranslateContentType Translatable => TextContentTypes.Translate;
    

    public static void Bootstrap() {}
}