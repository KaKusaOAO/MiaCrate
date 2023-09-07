using MiaCrate.Client.Systems;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Platform;

using NativeMonitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

public unsafe class Monitor
{
    public NativeMonitor* Handle { get; }
    private readonly List<VideoMode> _videoModes = new();
    private int _x;
    private int _y;
    public VideoMode CurrentMode { get; private set; }

    public int X => _x;
    public int Y => _y;

    public Monitor(NativeMonitor* handle)
    {
        Handle = handle;
        RefreshVideoModes();
    }

    public static MonitorCreator Create => v => new Monitor(v);

    public void RefreshVideoModes()
    {
        RenderSystem.AssertInInitPhase();
        _videoModes.Clear();
        
        var modes = GLFW.GetVideoModes(Handle);
        foreach (var mode in modes)
        {
            if (mode is {RedBits: >= 8, GreenBits: >= 8, BlueBits: >= 8})
            {
                _videoModes.Add(mode);
            }
        }

        GLFW.GetMonitorPos(Handle, out _x, out _y);
        CurrentMode = *GLFW.GetVideoMode(Handle);
    }

    public VideoMode GetPreferredVideoMode(VideoMode? videoMode)
    {
        RenderSystem.AssertInInitPhase();
        if (videoMode.HasValue)
        {
            var mode = videoMode.Value;
            foreach (var m in _videoModes)
            {
                if (VideoModeHelper.Equals(m, mode)) return m;
            }
        }

        return CurrentMode;
    }

    public int GetVideoModeIndex(VideoMode videoMode)
    {
        RenderSystem.AssertInInitPhase();
        return _videoModes.IndexOf(videoMode);
    }

    public VideoMode GetMode(int index) => _videoModes[index];

    public int GetModeCount() => _videoModes.Count;
}