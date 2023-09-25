using System.Text.RegularExpressions;
using MiaCrate.Client.Oshi;
using MiaCrate.Client.Systems;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Platform;

public static unsafe class GLX
{
    private static string? _cpuInfo;
    
    public static void Init(int i, bool bl)
    {
        RenderSystem.AssertInInitPhase();

        try
        {
            var processor = new SystemInfo().Hardware.Processor;
            _cpuInfo = Regex.Replace($"{processor.LogicalProcessorCount}x {processor.ProcessorIdentifier.Name}",
                "\\s+", " ").Trim();
        }
        catch // (Exception ex)
        {
            // Logger.Warn("Failed to fetch CPU info");
            // Logger.Warn(ex);
        }
    }

    public static Func<long> InitGlfw()
    {
        RenderSystem.AssertInInitPhase();
        if (!GLFW.Init())
        {
            throw new Exception("Failed to initialize GLFW");
        }

        return () => (long) (GLFW.GetTime() * 1.0e9);
    }

    public static string OpenGlVersion
    {
        get
        {
            RenderSystem.AssertOnRenderThread();
            if (GLFW.GetCurrentContext() == null) return "NO CONTEXT";

            var renderer = GlStateManager.GetString(StringName.Renderer);
            var version = GlStateManager.GetString(StringName.Version);
            var vendor = GlStateManager.GetString(StringName.Vendor);
            return $"{renderer} GL version {version}, {vendor}";
        }
    }

    public static string CpuInfo => _cpuInfo ?? "<unknown>";
    
    public static int GetRefreshRate(Window window)
    {
        RenderSystem.AssertOnRenderThread();
        var monitor = GLFW.GetWindowMonitor(window.Handle);
        if (monitor == null)
        {
            monitor = GLFW.GetPrimaryMonitor();
        }

        var videoMode = monitor == null ? null : GLFW.GetVideoMode(monitor);
        return videoMode == null ? 0 : videoMode->RefreshRate;
    }
}