using System.Text;
using MiaCrate.Texts;
using Mochi.Core;
using Mochi.Utils;

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

    public static bool IterateFormatted(string str, Style style, IFormattedCharSink sink) =>
        IterateFormatted(str, 0, style, sink);
    
    public static bool IterateFormatted(string str, int i, Style style, IFormattedCharSink sink) =>
        IterateFormatted(str, 0, style, style, sink);
    
    public static bool IterateFormatted(string str, int i, Style style, Style style2, IFormattedCharSink sink)
    {
        var j = str.Length;
        var style3 = style;

        for (var k = i; k < j; k++)
        {
            var c = str[k];
            char d;

            if (c == ChatFormatting.PrefixCode)
            {
                if (k + 1 >= j) break;

                d = str[k + 1];
                var formatting = ChatFormatting.GetByCode(d);
                if (formatting != null)
                {
                    style3 = formatting == ChatFormatting.Reset ? style2 : style3.ApplyLegacyFormat(formatting);
                }

                k++;
            } 
            else if (char.IsHighSurrogate(c))
            {
                if (k + 1 >= j)
                {
                    if (!sink.Accept(k, style3, ReplacementChar))
                        return false;

                    break;
                }

                d = str[k + 1];
                if (char.IsLowSurrogate(d))
                {
                    if (!sink.Accept(k, style3, char.ConvertToUtf32(c, d)))
                        return false;

                    k++;
                }
                else if (!sink.Accept(k, style3, ReplacementChar))
                    return false;
            }
            else if (!FeedChar(style3, sink, k, c))
                return false;
        }

        return true;
    }

    public static bool IterateFormatted(IFormattedText text, Style style, IFormattedCharSink sink)
    {
        return text.Visit((s, str) => 
            IterateFormatted(str, 0, s, sink) 
                ? Optional.Empty<Unit>() 
                : IFormattedText.StopIteration, style).IsEmpty;
    }
}