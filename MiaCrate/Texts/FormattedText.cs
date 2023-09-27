using Mochi.Texts;

namespace MiaCrate.Texts;

public static class TranslateText
{
    public static MutableComponent Of(string format, params IComponent[] texts)
    {
        var content = new TranslatableContent(format, null, texts);
        return new MutableComponent(content);
    }

    public static MutableComponent Fallback(string format, string fallback, params IComponent[] texts)
    {
        var content = new TranslatableContent(format, fallback, texts);
        return new MutableComponent(content);
    }
}