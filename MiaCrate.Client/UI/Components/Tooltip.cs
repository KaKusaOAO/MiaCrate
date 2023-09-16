using MiaCrate.Client.UI.Narration;
using Mochi.Texts;

namespace MiaCrate.Client.UI;

public class Tooltip : INarrationSupplier
{
    private readonly IComponent _message;
    private readonly IComponent? _narration;

    private Tooltip(IComponent message, IComponent? narration)
    {
        _message = message;
        _narration = narration;
    }

    public static Tooltip Create(IComponent message) => new(message, message);

    public static Tooltip Create(IComponent message, IComponent? narration) => new(message, narration);
    
    public void UpdateNarration(INarrationElementOutput output)
    {
        if (_narration == null) return;
        output.Add(NarratedElementType.Hint, _narration);
    }
}