using System.Text;
using MiaCrate.Texts;

namespace MiaCrate;

public static class StringDecomposer
{
    private const char ReplacementChar = (char) 0xfffd;

    private static bool FeedChar(Style style, IFormattedCharSink sink, int i, char c)
    {
        return char.IsSurrogate(c)
            ? sink.Accept(i, style, ReplacementChar)
            : sink.Accept(i, style, c);
    }
    
    public static bool Iterate(string str, Style style, IFormattedCharSink sink)
    {
        var i = str.Length;

        for (var j = 0; j < i; j++)
        {
            var c = str[j];

            if (char.IsHighSurrogate(c))
            {
                if (j + 1 >= i)
                {
                    if (!sink.Accept(j, style, ReplacementChar)) 
                        return false;
                    
                    break;
                }

                var d = str[j + 1];
                if (char.IsLowSurrogate(d))
                {
                    if (!sink.Accept(j, style, char.ConvertToUtf32(c, d)))
                        return false;

                    j++;
                } 
                else if (!sink.Accept(j, style, ReplacementChar))
                {
                    return false;
                }
            }
            else if (!FeedChar(style, sink, j, c))
            {
                return false;
            }
        }

        return true;
    }
    
    public static bool IterateBackwards(string str, Style style, IFormattedCharSink sink)
    {
        var i = str.Length;

        for (var j = i - 1; j >= 0; j--)
        {
            var c = str[j];

            if (char.IsLowSurrogate(c))
            {
                if (j - 1 < 0)
                {
                    if (!sink.Accept(j, style, ReplacementChar)) 
                        return false;
                    
                    break;
                }

                var d = str[j - 1];
                if (char.IsHighSurrogate(d))
                {
                    if (!sink.Accept(--j, style, char.ConvertToUtf32(d, c)))
                        return false;
                } 
                else if (!sink.Accept(j, style, ReplacementChar))
                {
                    return false;
                }
            }
            else if (!FeedChar(style, sink, j, c))
            {
                return false;
            }
        }

        return true;
    }
}