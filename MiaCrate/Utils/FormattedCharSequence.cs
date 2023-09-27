using MiaCrate.Texts;

namespace MiaCrate;

public delegate bool FormattedCharSequence(IFormattedCharSink sink);

public static class FormattedCharSequences
{
    public static FormattedCharSequence Empty { get; } = _ => true;

    public static FormattedCharSequence Codepoint(int i, Style style) => 
        s => s.Accept(0, style, i);

    public static FormattedCharSequence Forward(string str, Style style) =>
        string.IsNullOrEmpty(str) ? Empty : s => StringDecomposer.Iterate(str, style, s);
    
    
    public static FormattedCharSequence Forward(string str, Style style, Func<int, int> transform) =>
        string.IsNullOrEmpty(str) ? Empty : 
            s => StringDecomposer.Iterate(str, style, DecorateOutput(s, transform));
    
    public static FormattedCharSequence Backward(string str, Style style) =>
        string.IsNullOrEmpty(str) ? Empty : s => StringDecomposer.IterateBackwards(str, style, s);
    
    
    public static FormattedCharSequence Backward(string str, Style style, Func<int, int> transform) =>
        string.IsNullOrEmpty(str) ? Empty : 
            s => StringDecomposer.IterateBackwards(str, style, DecorateOutput(s, transform));
    
    private static IFormattedCharSink DecorateOutput(IFormattedCharSink sink, Func<int, int> transform) =>
        IFormattedCharSink.Create((i, style, codepoint) =>
            sink.Accept(i, style, transform(codepoint)));
}