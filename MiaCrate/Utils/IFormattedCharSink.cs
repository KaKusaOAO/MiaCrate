using MiaCrate.Texts;

namespace MiaCrate;

public interface IFormattedCharSink
{
    bool Accept(int i, Style style, int codepoint);
}