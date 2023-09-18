using System.Runtime.InteropServices;
using System.Text;
using MiaCrate.Client.Platform;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NativeWindow = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client;

public class MouseHandler
{
    private readonly Game _game;
    private bool _mouseGrabbed;
    private bool _ignoreFirstMove = true;

    public double XPos { get; private set; }
    public double YPos { get; private set; }

    public MouseHandler(Game game)
    {
        _game = game;
    }

    public void GrabMouse()
    {
        if (!_game.IsWindowActive) return;
        if (_mouseGrabbed) return;
        if (!Game.OnMacOs) KeyMapping.SetAll();
    }

    public unsafe void Setup(NativeWindow* handle)
    {
        InputConstants.SetupMouseCallbacks(handle, 
            (window, d, d1) => _game.Execute(() => OnMove(window, d, d1)),
            (window, button, action, mods) => _game.Execute(() => OnPress(window, button, action, mods)),
            (window, x, y) =>  _game.Execute(() => OnScroll(window, x, y)),
            (window, count, paths) =>
            {
                var list = new List<string>(count);
                for (var i = 0; i < count; i++)
                {
                    var ptr = *(paths + i);
                    var str = Marshal.PtrToStringUTF8((IntPtr) ptr)!;
                    list.Add(str);
                }
                _game.Execute(() => OnDrop(window, list));
            });
    }

    private unsafe void OnMove(NativeWindow* handle, double x, double y)
    {
        if (handle != Game.Instance.Window.Handle) return;
        if (_ignoreFirstMove)
        {
            XPos = x;
            YPos = y;
            _ignoreFirstMove = false;
        }

        var screen = _game.Screen;
        if (screen != null && _game.Overlay == null)
        {
            // ...
        }
    }
    
    private unsafe void OnPress(NativeWindow* handle, MouseButton button, InputAction action, KeyModifiers mods)
    {
        if (handle != Game.Instance.Window.Handle) return;

    }
    
    private unsafe void OnScroll(NativeWindow* handle, double x, double y)
    {
        if (handle != Game.Instance.Window.Handle) return;

    }
    
    private unsafe void OnDrop(NativeWindow* handle, List<string> paths)
    {
        if (handle != Game.Instance.Window.Handle) return;

    }
}