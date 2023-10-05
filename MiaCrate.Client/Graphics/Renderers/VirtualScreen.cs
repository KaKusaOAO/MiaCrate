using MiaCrate.Client.Platform;
using Monitor = MiaCrate.Client.Platform.Monitor;

namespace MiaCrate.Client.Graphics;

public class VirtualScreen : IDisposable
{
    private readonly Game _game;
    private readonly ScreenManager _screenManager;

    public VirtualScreen(Game game)
    {
        _game = game;
        _screenManager = new ScreenManager(Monitor.Create);
    }

    public Window NewWindow(DisplayData data, string? str, string str2) => new(_screenManager, data, str, str2);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _screenManager.Dispose();
    }
}