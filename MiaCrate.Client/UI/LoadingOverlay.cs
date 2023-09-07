using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.UI;

public class LoadingOverlay : Overlay
{
    private readonly Game _game;
    private readonly IReloadInstance _reload;
    private readonly Action<IOptional<Exception>> _onFinish;
    private readonly bool _fadeIn;

    public LoadingOverlay(Game game, IReloadInstance reload, Action<IOptional<Exception>> onFinish, bool fadeIn)
    {
        _game = game;
        _reload = reload;
        _onFinish = onFinish;
        _fadeIn = fadeIn;
    }
    
    public override void Render(GuiGraphics graphics, int i, int j, float f)
    {
        
    }
}