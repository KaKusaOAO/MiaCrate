using MiaCrate.Texts;

namespace MiaCrate;

public interface IFormattedCharSink
{
    bool Accept(int i, Style style, int codepoint);

    public static IFormattedCharSink Create(Func<int, Style, int, bool> func) => new Immediate(func);

    private class Immediate : IFormattedCharSink
    {
        private readonly Func<int, Style, int, bool> _func;

        public Immediate(Func<int, Style, int, bool> func)
        {
            _func = func;
        }

        public bool Accept(int i, Style style, int codepoint) => _func(i, style, codepoint);
    }
}