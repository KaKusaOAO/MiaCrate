using Mochi.Texts;

namespace MiaCrate.Texts;

public class MiaStyleProvider : IStyleProvider
{
    public IStyle CreateEmpty() => Style.Empty;
}