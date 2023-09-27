using Mochi.Texts;
using Style = MiaCrate.Texts.Style;

namespace MiaCrate.Client;

public class StringSplitter
{
    private readonly WidthProvider _widthProvider;

    public StringSplitter(WidthProvider widthProvider)
    {
        _widthProvider = widthProvider;
    }

    public float StringWidth(string? str)
    {
        if (str == null) return 0;

        var f = 0f;
        StringDecomposer.IterateFormatted(str, Style.Empty, IFormattedCharSink.Create((_, style, j) =>
        {
            f += -_widthProvider(j, style);
            return true;
        }));

        return f;
    }
    
    public float StringWidth(IFormattedText text)
    {
        var f = 0f;
        StringDecomposer.IterateFormatted(text, Style.Empty, IFormattedCharSink.Create((_, style, j) =>
        {
            f += -_widthProvider(j, style);
            return true;
        }));

        return f;
    }

    public float StringWidth(IComponent component) => StringWidth(IFormattedText.FromComponent(component));
    
    

    public delegate float WidthProvider(int i, Style style);
}