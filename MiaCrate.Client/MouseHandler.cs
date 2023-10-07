using System.Runtime.InteropServices;
using System.Text;
using MiaCrate.Client.Platform;
using MiaCrate.Client.UI;
using MiaCrate.Client.UI.Screens;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NativeWindow = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client;

public class MouseHandler
{
    private const MouseButton ButtonNotAssigned = (MouseButton) (-1);
    
    private readonly Game _game;
    private bool _ignoreFirstMove = true;
    private MouseButton _activeButton;
    private double _mousePressedTime;
    private double _lastMouseEventTime;
    private double _accumulatedDX;
    private double _accumulatedDY;
    private int _fakeRightMouse;
    private int _clickDepth;
    private bool _isLeftPressed;
    private bool _isMiddlePressed;
    private bool _isRightPressed;

    public double XPos { get; private set; }
    public double YPos { get; private set; }
    public bool IsMouseGrabbed { get; private set; }

    public MouseHandler(Game game)
    {
        _game = game;
    }

    public void GrabMouse()
    {
        if (!_game.IsWindowActive) return;
        if (IsMouseGrabbed) return;
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
            var f = x * _game.Window.GuiScaledWidth / _game.Window.ScreenWidth;
            var g = y * _game.Window.GuiScaledHeight / _game.Window.ScreenHeight;
            Screen.WrapScreenError(() => screen.MouseMoved(f, g), 
                "mouseMoved event handler", screen.GetType().Name);

            if (_activeButton != ButtonNotAssigned && _mousePressedTime > 0.0)
            {
                var h = (x - XPos) * _game.Window.GuiScaledWidth / _game.Window.ScreenWidth;
                var i = (y - YPos) * _game.Window.GuiScaledHeight / _game.Window.ScreenHeight;
                Screen.WrapScreenError(() => screen.MouseDragged(f, g, _activeButton, h, i),
                    "mouseDragged event handler", screen.GetType().Name);
            }

            screen.AfterMouseMove();
        }

        if (IsMouseGrabbed && _game.IsWindowActive)
        {
            _accumulatedDX += x - XPos;
            _accumulatedDY += y - YPos;
        }

        TurnPlayer();
        XPos = x;
        YPos = y;
    }

    public void TurnPlayer()
    {
        var d = Blaze3D.Time;
        var e = d - _lastMouseEventTime;
        _lastMouseEventTime = d;

        if (!IsMouseGrabbed || !_game.IsWindowActive)
        {
            _accumulatedDX = 0.0;
            _accumulatedDY = 0.0;
            return;
        }
        
        Util.LogFoobar();
    }

    private unsafe void OnPress(NativeWindow* handle, MouseButton button, InputAction action, KeyModifiers mods)
    {
        if (handle != Game.Instance.Window.Handle) return;

        if (_game.Screen != null)
        {
            _game.LastInputType = InputType.Mouse;
        }

        var bl = action == InputAction.Press;
        if (Game.OnMacOs && button == MouseButton.Left)
        {
            if (bl)
            {
                if (mods.HasFlag(KeyModifiers.Control))
                {
                    button = MouseButton.Right;
                    ++_fakeRightMouse;
                }
            } else if (_fakeRightMouse > 0)
            {
                button = MouseButton.Right;
                --_fakeRightMouse;
            }
        }

        if (bl)
        {
            if (_game.Options.Touchscreen.Value && _clickDepth++ > 0) return;
            _activeButton = button;
            _mousePressedTime = Blaze3D.Time;
        } 
        else if (_activeButton != ButtonNotAssigned)
        {
            if (_game.Options.Touchscreen.Value && --_clickDepth > 0) return;
            _activeButton = ButtonNotAssigned;
        }

        var bls = false;
        if (_game.Overlay == null)
        {
            if (_game.Screen == null)
            {
                if (!IsMouseGrabbed && bl)
                {
                    GrabMouse();
                }
            }
            else
            {
                var d = XPos * _game.Window.GuiScaledWidth / _game.Window.ScreenWidth;
                var e = YPos * _game.Window.GuiScaledHeight / _game.Window.ScreenHeight;
                var screen = _game.Screen!;

                if (bl)
                {
                    screen.AfterMouseAction();
                    Screen.WrapScreenError(() =>
                    {
                        bls = screen.MouseClicked(d, e, button);
                    }, "mouseClicked event handler", screen.GetType().Name);
                }
                else
                {
                    Screen.WrapScreenError(() =>
                    {
                        bls = screen.MouseReleased(d, e, button);
                    }, "mouseReleased event handler", screen.GetType().Name);
                }
            }
        }

        if (!bls && _game.Screen == null && _game.Overlay == null)
        {
            if (button == MouseButton.Left)
            {
                _isLeftPressed = bl;
            }
            else if (button == MouseButton.Middle)
            {
                _isMiddlePressed = bl;
            }
            else if (button == MouseButton.Right)
            {
                _isRightPressed = bl;
            }
            
            KeyMapping.Set(InputConstants.KeyType.Mouse.GetOrCreate((int) button), bl);
            if (bl)
            {
                Util.LogFoobar();
            }
        }
    }
    
    private unsafe void OnScroll(NativeWindow* handle, double x, double y)
    {
        if (handle != Game.Instance.Window.Handle) return;

    }
    
    private unsafe void OnDrop(NativeWindow* handle, List<string> paths)
    {
        if (handle != Game.Instance.Window.Handle) return;

    }

    public void ReleaseMouse()
    {
        if (!IsMouseGrabbed) return;
        IsMouseGrabbed = false;
        XPos = _game.Window.ScreenWidth / 2.0;
        YPos = _game.Window.ScreenHeight / 2.0;

        unsafe
        {
            InputConstants.GrabOrReleaseMouse(_game.Window.Handle, CursorModeValue.CursorNormal, XPos, YPos);
        }
    }
}