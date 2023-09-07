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
    private static event GLFWCallbacks.ErrorCallback? OnError;
    private static event GLFWCallbacks.FramebufferSizeCallback? OnFramebufferResize;
    private static event GLFWCallbacks.WindowPosCallback? OnWindowPosUpdate;
    private static event GLFWCallbacks.WindowSizeCallback? OnWindowResize;
    private static event GLFWCallbacks.WindowFocusCallback? OnWindowFocusUpdate;
    private static event GLFWCallbacks.CursorEnterCallback? OnCursorEnter;
    
    public event WindowActiveEventHandler? WindowActiveChanged;
    public event DisplayResizeEventHandler? DisplayResized;
    public event CursorEnterEventHandler? CursorEntered;

    public NativeWindow* Handle { get; }
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
    private int _guiScaledWidth;
    private int _guiScaledHeight;

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
    
    private int _windowedX;
    private int _windowedY;
    private int _x;
    private int _y;

    public int X => _x;
    public int Y => _y;

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
        
        OnFramebufferResize += WindowOnFramebufferResize;
        OnWindowPosUpdate += OnMove;
        OnWindowResize += OnResize;
        OnWindowFocusUpdate += OnFocus;
        OnCursorEnter += OnEnter;
        GLFW.MakeContextCurrent(Handle);
        
        var context = new GLFWBindingsContext();
        OpenTK.Graphics.ES11.GL.LoadBindings(context);
        OpenTK.Graphics.ES20.GL.LoadBindings(context);
        OpenTK.Graphics.ES30.GL.LoadBindings(context);
        OpenTK.Graphics.OpenGL.GL.LoadBindings(context);
        OpenTK.Graphics.OpenGL4.GL.LoadBindings(context);
        
        SetMode();
        RefreshFramebufferSize();
        GLFW.SetFramebufferSizeCallback(Handle, DelegateFramebufferResizeCallback);
        GLFW.SetWindowPosCallback(Handle, DelegateWindowPosCallback);
        GLFW.SetWindowSizeCallback(Handle, DelegateWindowSizeCallback);
        GLFW.SetWindowFocusCallback(Handle, DelegateWindowFocusCallback);
        GLFW.SetCursorEnterCallback(Handle, DelegateCursorEnterCallback);
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

    private static void DelegateCursorEnterCallback(NativeWindow* window, bool entered) =>
        OnCursorEnter?.Invoke(window, entered);

    private static void DelegateWindowFocusCallback(NativeWindow* window, bool focused) => 
        OnWindowFocusUpdate?.Invoke(window, focused);

    private static void DelegateWindowSizeCallback(NativeWindow* window, int width, int height) => 
        OnWindowResize?.Invoke(window, width, height);

    private static void DelegateWindowPosCallback(NativeWindow* window, int x, int y) =>
        OnWindowPosUpdate?.Invoke(window, x, y);

    private static void DelegateFramebufferResizeCallback(NativeWindow* window, int width, int height) =>
        OnFramebufferResize?.Invoke(window, width, height);

    public void SetErrorSection(string section)
    {
        _errorSection = section;
    }

    private void SetBootErrorCallback()
    {
        RenderSystem.AssertInInitPhase();
        OnError += BootCrash;
        GLFW.SetErrorCallback(DelegateErrorCallback);
    }

    private static void BootCrash(ErrorCode error, string description)
    {
        RenderSystem.AssertInInitPhase();
        var str = $"GLFW error {error}: {description}";
        
        // TODO: Show the message box
        Logger.Error(str);
        throw new WindowInitFailedException(str);
    }

    private static void DelegateErrorCallback(ErrorCode error, string description) =>
        OnError?.Invoke(error, description);

    public void Dispose()
    {
        RenderSystem.AssertOnRenderThread();
        GLFW.DestroyWindow(Handle);
        GLFW.Terminate();
    }
}