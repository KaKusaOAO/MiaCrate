using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MiaCrate.Client.Systems;
using Mochi.Utils;
using SDL2;
using Veldrid;
using Veldrid.OpenGL;

namespace MiaCrate.Client.Platform;

public delegate void WindowActiveEventHandler(bool active);

public delegate void DisplayResizeEventHandler();

public delegate void CursorEnterEventHandler();

public unsafe class Window : IDisposable
{
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
    
    private readonly ScreenManager _screenManager;
    private string _errorSection = "";
    private SDL.SDL_DisplayMode? _preferredFullscreenVideoMode;

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

    public IntPtr Handle { get; }
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
        
        var monitor = screenManager.GetMonitor(0);
        _windowedWidth = _width = Math.Max(displayData.Width, 1);
        _windowedHeight = _height = Math.Max(displayData.Height, 1);

        var flags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                    SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
        
        if (SharedConstants.UseHighDpi) 
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

        GraphicsBackend backend;
        // if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
        // {
        //     flags |= SDL.SDL_WindowFlags.SDL_WINDOW_METAL;
        //     backend = GraphicsBackend.Metal;
        // } 
        // else 
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_VULKAN;
            backend = GraphicsBackend.Vulkan;
        }
        else
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11))
        {
            // Missing flag for Direct3D
            backend = GraphicsBackend.Direct3D11;
        }
        else
        {
            flags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;
            backend = GraphicsBackend.OpenGL;
        }

        if (SDL.SDL_GetDisplayBounds(0, out var rect) != 0)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }

        var x = rect.w / 2 - _width / 2;
        var y = rect.h / 2 - _height / 2;
        var handle = SDL.SDL_CreateWindow(str2, x, y, _width, _height, flags);
        if (handle == 0)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }

        Handle = handle;
        if (monitor != null)
        {
            var videoMode = monitor.GetPreferredVideoMode(_fullscreen ? _preferredFullscreenVideoMode : null);
            _windowedX = _x = monitor.X + (videoMode.w - _width) / 2;
            _windowedY = _y = monitor.Y + (videoMode.h - _height) / 2;
        }
        else
        {
            SDL.SDL_GetWindowPosition(Handle, out _x, out _y);
            _windowedX = _x;
            _windowedY = _y;
        }

        GraphicsDevice device;
        var options = new GraphicsDeviceOptions
        {
            PreferStandardClipSpaceYDirection = true,
            Debug = true,
        };

        var swapchainSource = GetSwapchainSource();
        var swapchainDescription = new SwapchainDescription(
            swapchainSource, (uint) _width, (uint) _height, options.SwapchainDepthFormat, options.SyncToVerticalBlank);
        
        switch (backend)
        {
            case GraphicsBackend.Metal:
                device = GraphicsDevice.CreateMetal(options, swapchainDescription);
                break;
            case GraphicsBackend.Vulkan:
                device = GraphicsDevice.CreateVulkan(options, swapchainDescription);
                break;
            case GraphicsBackend.Direct3D11:
                device = GraphicsDevice.CreateD3D11(options, swapchainDescription);
                break;
            case GraphicsBackend.OpenGL:
            {
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS,
                    (int) (SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG | SDL.SDL_GLcontext.SDL_GL_CONTEXT_DEBUG_FLAG));
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
                SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 2);
                
                var context = SDL.SDL_GL_CreateContext(Handle);
                
                device = GraphicsDevice.CreateOpenGL(options, new OpenGLPlatformInfo(
                    context, 
                    SDL.SDL_GL_GetProcAddress,
                    c => SDL.SDL_GL_MakeCurrent(Handle, c),
                    SDL.SDL_GL_GetCurrentContext,
                    () => SDL.SDL_GL_MakeCurrent(0, 0),
                    SDL.SDL_GL_DeleteContext,
                    () => SDL.SDL_GL_SwapWindow(Handle),
                    s => SDL.SDL_GL_SetSwapInterval(s ? 1 : 0)),
                    (uint) _width, (uint) _height);
                break;
            }
            default:
                throw new NotSupportedException("Cannot create graphic device");
        }
        
        GlStateManager.Init(device);
        SetMode();
        RefreshFramebufferSize();
    }

    public void Tick()
    {
        PollSDLEvents();
    }

    private const int EventsPerPeep = 64;
    private readonly SDL.SDL_Event[] _events = new SDL.SDL_Event[EventsPerPeep];

    // ReSharper disable once InconsistentNaming
    public static Action<SDL.SDL_Event>? ReceiveSDLEvent;

    private void PollSDLEvents()
    {
        SDL.SDL_PumpEvents();

        int eventsRead;

        do
        {
            eventsRead = SDL.SDL_PeepEvents(_events, EventsPerPeep, SDL.SDL_eventaction.SDL_GETEVENT,
                SDL.SDL_EventType.SDL_FIRSTEVENT, SDL.SDL_EventType.SDL_LASTEVENT);

            for (var i = 0; i < eventsRead; i++)
            {
                var ev = _events[i];
                HandleEvent(ev);
                ReceiveSDLEvent?.Invoke(ev);
            }
        } while (eventsRead == EventsPerPeep);
    }

    private void HandleEvent(SDL.SDL_Event ev)
    {
        switch (ev.type)
        {
            case SDL.SDL_EventType.SDL_WINDOWEVENT:
                HandleWindowEvent(ev.window);
                break;
        }
    }

    private void HandleWindowEvent(SDL.SDL_WindowEvent ev)
    {
        switch (ev.windowEvent)
        {
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                OnResize(Handle, ev.data1, ev.data2);
                GlStateManager.SubmitCommands();

                int width, height;
                switch (GlStateManager.Device.BackendType)
                {
                    case GraphicsBackend.Metal:
                        SDL.SDL_Metal_GetDrawableSize(Handle, out width, out height);
                        break;
                    case GraphicsBackend.Vulkan:
                        SDL.SDL_Vulkan_GetDrawableSize(Handle, out width, out height);
                        break;
                    case GraphicsBackend.OpenGL:
                    case GraphicsBackend.OpenGLES:
                        SDL.SDL_GL_GetDrawableSize(Handle, out width, out height);
                        break;
                    default:
                        width = ev.data1;
                        height = ev.data2;
                        break;
                }
                GlStateManager.Device.ResizeMainWindow((uint) width, (uint) height);
                WindowOnFramebufferResize(Handle, width, height);
                break;
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                OnMove(Handle, ev.data1, ev.data2);
                break;
            // case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                OnFocus(Handle, true);
                break;
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                OnFocus(Handle, false);
                break;
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                OnEnter(Handle, true);
                break;
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                OnEnter(Handle, false);
                break;
            case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                ShouldClose = true;
                break;
        }
    }

    private SwapchainSource GetSwapchainSource()
    {
        var sysWmInfo = new SDL.SDL_SysWMinfo();
        SDL.SDL_GetVersion(out sysWmInfo.version);
        SDL.SDL_GetWindowWMInfo(Handle, ref sysWmInfo);
        switch (sysWmInfo.subsystem)
        {
            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                var w32Info = sysWmInfo.info.win;
                return SwapchainSource.CreateWin32(w32Info.window, w32Info.hinstance);
            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                var x11Info = sysWmInfo.info.x11;
                return SwapchainSource.CreateXlib(
                    x11Info.display,
                    x11Info.window);
            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WAYLAND:
                var wlInfo = sysWmInfo.info.wl;
                return SwapchainSource.CreateWayland(wlInfo.display, wlInfo.surface);
            case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                var cocoaInfo = sysWmInfo.info.cocoa;
                IntPtr nsWindow = cocoaInfo.window;
                return SwapchainSource.CreateNSWindow(nsWindow);
            default:
                throw new PlatformNotSupportedException("Cannot create a SwapchainSource for " + sysWmInfo.subsystem + ".");
        }
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
        var bl = SDL.SDL_GetWindowDisplayIndex(Handle) >= 0;
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
                Util.LogFoobar();
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

    public int FrameRateLimit { get; set; } = 144;

    public bool ShouldClose { get; private set; }

    private void OnEnter(IntPtr window, bool entered)
    {
        if (!entered) return;
        CursorEntered?.Invoke();
    }

    private void OnFocus(IntPtr window, bool focused)
    {
        if (window != Handle) return;
        WindowActiveChanged?.Invoke(focused);
    }

    private void OnResize(IntPtr window, int width, int height)
    {
        _width = width;
        _height = height;
    }

    private void OnMove(IntPtr window, int x, int y)
    {
        _x = x;
        _y = y;
    }

    private void WindowOnFramebufferResize(IntPtr window, int width, int height)
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
        int width, height;
        var backend = GlStateManager.Device.BackendType;
        switch (backend)
        {
            case GraphicsBackend.Metal:
                SDL.SDL_Metal_GetDrawableSize(Handle, out width, out height);
                break;
            case GraphicsBackend.Vulkan:
                SDL.SDL_Vulkan_GetDrawableSize(Handle, out width, out height);
                break;
            case GraphicsBackend.OpenGL:
            case GraphicsBackend.OpenGLES:
                SDL.SDL_GL_GetDrawableSize(Handle, out width, out height);
                break;
            default:
                SDL.SDL_GetWindowSize(Handle, out width, out height);
                break;
        }
        
        // GLFW.GetFramebufferSize(Handle, out var width, out var height);
        _framebufferWidth = Math.Max(width, 1);
        _framebufferHeight = Math.Max(height, 1);
        DisplayResized?.Invoke();
    }

    public void SetErrorSection(string section)
    {
        _errorSection = section;
    }

    private void SetBootErrorCallback()
    {
        RenderSystem.AssertInInitPhase();
        // _delegateErrorCallback = BootCrash;
        // GLFW.SetErrorCallback(_delegateErrorCallback);
    }

    public void SetTitle(string title)
    {
        SDL.SDL_SetWindowTitle(Handle, title);
    }

    private static void BootCrash(/*ErrorCode error, string description*/)
    {
        RenderSystem.AssertInInitPhase();
        // var str = $"GLFW error {error}: {description}";
        //
        // // TODO: Show the message box
        // Logger.Error(str);
        // throw new WindowInitFailedException(str);
    }

    public void Dispose()
    {
        RenderSystem.AssertOnRenderThread();
        SDL.SDL_DestroyWindow(Handle);
        // GLFW.DestroyWindow(Handle);
        // GLFW.Terminate();
    }
}