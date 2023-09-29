using System.Text.RegularExpressions;
using MiaCrate.Client.Oshi;
using MiaCrate.Client.Systems;
using MiaCrate.Client.Utils;
using SDL2;

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
        if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) < 0)
        {
            throw new Exception("Failed to initialize SDL2");
        }

        return () => (long) (SDL.SDL_GetTicks() / 1000.0 * 1.0e9);
    }

    public static string OpenGlVersion
    {
        get
        {
            RenderSystem.AssertOnRenderThread();
            // if (GLFW.GetCurrentContext() == null) return "NO CONTEXT";
            //
            // var renderer = GlStateManager.GetString(StringName.Renderer);
            // var version = GlStateManager.GetString(StringName.Version);
            // var vendor = GlStateManager.GetString(StringName.Vendor);
            // return $"{renderer} GL version {version}, {vendor}";

            var device = GlStateManager.Device;
            var renderer = device.DeviceName;
            var version = device.ApiVersion.ToString();
            var vendor = device.VendorName;
            var backend = device.BackendType;
            return $"{renderer} {backend} version {version}, {vendor}";
        }
    }

    public static string CpuInfo => _cpuInfo ?? "<unknown>";
    
    public static int GetRefreshRate(Window window)
    {
        RenderSystem.AssertOnRenderThread();
        var monitor = Math.Max(0, SDL.SDL_GetWindowDisplayIndex(window.Handle));

        if (SDL.SDL_GetCurrentDisplayMode(monitor, out var mode) < 0) return 0;
        return mode.refresh_rate;
    }
}