using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;
using NativeWindow = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client.Platform;

public delegate void WindowActiveEventHandler(bool active);

public delegate void DisplayResizeEventHandler();

public delegate void CursorEnterEventHandler();

public unsafe class Window : IDisposable
{
    private static GLFWCallbacks.ErrorCallback? _delegateErrorCallback;
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private static GLFWCallbacks.FramebufferSizeCallback? _delegateFramebufferSizeCallback;
    private static GLFWCallbacks.WindowPosCallback? _delegateWindowPosCallback;
    private static GLFWCallbacks.WindowSizeCallback? _delegateWindowSizeCallback;
    private static GLFWCallbacks.WindowFocusCallback? _delegateWindowFocusCallback;
    private static GLFWCallbacks.CursorEnterCallback? _delegateCursorEnterCallback;
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
    
    private readonly ScreenManager _screenManager;
    private string _errorSection = "";
    private VideoMode? _preferredFullscreenVideoMode;

    private bool _fullscreen;
    private bool _actuallyFullscreen;

    private int _windowedWidth;
    private int _windowedHeight;
    private int _width;
    private int _height;

    private int _framebufferWidth;
    private int _framebufferHeight;

    private int _windowedX;
    private int _windowedY;
    private int _x;
    private int _y;

    public event WindowActiveEventHandler? WindowActiveChanged;
    public event DisplayResizeEventHandler? DisplayResized;
    public event CursorEnterEventHandler? CursorEntered;

    public NativeWindow* Handle { get; }
    public int X => _x;
    public int Y => _y;

    public int Width
    {
        get => _framebufferWidth;
        set => _framebufferWidth = value;
    }

    public int Height
    {
        get => _framebufferHeight;
        set => _framebufferHeight = value;
    }
    public double GuiScale { get; private set; }

    public int ScreenWidth => _width;
    public int ScreenHeight => _height;
    public int GuiScaledWidth { get; private set; }

    public int GuiScaledHeight { get; private set; }

    public Window(ScreenManager screenManager, DisplayData displayData, string? str, string str2)
    {
        RenderSystem.AssertInInitPhase();
        _screenManager = screenManager;
        SetBootErrorCallback();
        SetErrorSection("Pre startup");

        var fVideoMode = VideoModeHelper.Read(str);
        if (fVideoMode != null)
        {
            _preferredFullscreenVideoMode = fVideoMode;
        } else if (displayData is {FullscreenWidth: not null, FullscreenHeight: not null})
        {
            _preferredFullscreenVideoMode = VideoModeHelper.Create(displayData.FullscreenWidth.Value,
                displayData.FullscreenHeight.Value, 8, 8, 8, 60);
        }
        else
        {
            _preferredFullscreenVideoMode = null;
        }

        _actuallyFullscreen = _fullscreen = displayData.IsFullscreen;
        var monitor = screenManager.GetMonitor(GLFW.GetPrimaryMonitor());
        _windowedWidth = _width = Math.Max(displayData.Width, 1);
        _windowedHeight = _height = Math.Max(displayData.Height, 1);
        
        GLFW.DefaultWindowHints();
        GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
        GLFW.WindowHint(WindowHintContextApi.ContextCreationApi, ContextApi.NativeContextApi);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 2);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        GLFW.WindowHint(WindowHintBool.OpenGLForwardCompat, true);
        
        Handle = GLFW.CreateWindow(_width, _height, str2, _fullscreen && monitor != null ? monitor.Handle : null, null);
        if (monitor != null)
        {
            var videoMode = monitor.GetPreferredVideoMode(_fullscreen ? _preferredFullscreenVideoMode : null);
            _windowedX = _x = monitor.X + (videoMode.Width - _width) / 2;
            _windowedY = _y = monitor.Y + (videoMode.Height - _height) / 2;
        }
        else
        {
            GLFW.GetWindowPos(Handle, out _x, out _y);
            _windowedX = _x;
            _windowedY = _y;
        }
        GLFW.MakeContextCurrent(Handle);
        
        var context = new GLFWBindingsContext();
        OpenTK.Graphics.ES11.GL.LoadBindings(context);
        OpenTK.Graphics.ES20.GL.LoadBindings(context);
        OpenTK.Graphics.ES30.GL.LoadBindings(context);
        OpenTK.Graphics.OpenGL.GL.LoadBindings(context);
        OpenTK.Graphics.OpenGL4.GL.LoadBindings(context);
        
        SetMode();
        RefreshFramebufferSize();
        
        _delegateFramebufferSizeCallback = WindowOnFramebufferResize;
        _delegateWindowPosCallback = OnMove;
        _delegateWindowSizeCallback = OnResize;
        _delegateWindowFocusCallback = OnFocus;
        _delegateCursorEnterCallback = OnEnter;
        
        GLFW.SetFramebufferSizeCallback(Handle, _delegateFramebufferSizeCallback);
        GLFW.SetWindowPosCallback(Handle, _delegateWindowPosCallback);
        GLFW.SetWindowSizeCallback(Handle, _delegateWindowSizeCallback);
        GLFW.SetWindowFocusCallback(Handle, _delegateWindowFocusCallback);
        GLFW.SetCursorEnterCallback(Handle, _delegateCursorEnterCallback);
    }
    
    public void UpdateDisplay()
    {
        RenderSystem.FlipFrame(Handle);
    }

    public int CalculateScale(int i, bool bl)
    {
        int j;
        for (j = 1;
             j != i && j < _framebufferWidth && j < _framebufferHeight && _framebufferWidth / (j + 1) >= 320 &&
             _framebufferHeight / (j + 1) >= 240;
             j++)
        {
            
        }

        if (bl && j % 2 == 0) j++;
        return j;
    }

    public void SetGuiScale(double d)
    {
        GuiScale = d;

        var i = (int) (_framebufferWidth / d);
        GuiScaledWidth = _framebufferWidth / d > i ? i + 1 : i;

        var j = (int) (_framebufferHeight / d);
        GuiScaledHeight = _framebufferHeight / d > j ? j + 1 : j;
    }

    private void SetMode()
    {
        RenderSystem.AssertInInitPhase();
        var bl = GLFW.GetWindowMonitor(Handle) != null;
        if (_fullscreen)
        {
            var monitor = _screenManager.FindBestMonitor(this);
            if (monitor == null)
            {
                Logger.Warn("Failed to find suitable monitor for fullscreen mode");
                _fullscreen = false;
            }
            else
            {
                
            }
        }
    }

    public int RefreshRate
    {
        get
        {
            RenderSystem.AssertOnRenderThread();
            return GLX.GetRefreshRate(this);
        }
    }

    public int FrameRateLimit { get; set; } = 60;

    public bool ShouldClose => GLFW.WindowShouldClose(Handle);

    private void OnEnter(NativeWindow* window, bool entered)
    {
        if (!entered) return;
        CursorEntered?.Invoke();
    }

    private void OnFocus(NativeWindow* window, bool focused)
    {
        if (window != Handle) return;
        WindowActiveChanged?.Invoke(focused);
    }

    private void OnResize(NativeWindow* window, int width, int height)
    {
        _width = width;
        _height = height;
    }

    private void OnMove(NativeWindow* window, int x, int y)
    {
        _x = x;
        _y = y;
    }

    private void WindowOnFramebufferResize(NativeWindow* window, int width, int height)
    {
        if (window != Handle) return;
        if (width == 0 || height == 0) return;

        var k = Width;
        var m = Height;
        
        _framebufferWidth = width;
        _framebufferHeight = height;
        if (Width != k || Height != m)
        {
            DisplayResized?.Invoke();
        }
    }

    private void RefreshFramebufferSize()
    {
        GLFW.GetFramebufferSize(Handle, out var width, out var height);
        _framebufferWidth = Math.Max(width, 1);
        _framebufferHeight = Math.Max(height, 1);
    }

    public void SetErrorSection(string section)
    {
        _errorSection = section;
    }

    private void SetBootErrorCallback()
    {
        RenderSystem.AssertInInitPhase();
        _delegateErrorCallback = BootCrash;
        GLFW.SetErrorCallback(_delegateErrorCallback);
    }

    public void SetTitle(string title)
    {
        GLFW.SetWindowTitle(Handle, title);
    }

    private static void BootCrash(ErrorCode error, string description)
    {
        RenderSystem.AssertInInitPhase();
        var str = $"GLFW error {error}: {description}";
        
        // TODO: Show the message box
        Logger.Error(str);
        throw new WindowInitFailedException(str);
    }

    public void Dispose()
    {
        RenderSystem.AssertOnRenderThread();
        GLFW.DestroyWindow(Handle);
        GLFW.Terminate();
    }
}