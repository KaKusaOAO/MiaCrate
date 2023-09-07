using System.Runtime.CompilerServices;
using MiaCrate.Client.Systems;
using OpenTK.Windowing.GraphicsLibraryFramework;
using NativeMonitor = OpenTK.Windowing.GraphicsLibraryFramework.Monitor;

namespace MiaCrate.Client.Platform;

public unsafe delegate Monitor MonitorCreator(NativeMonitor* monitor);

public unsafe class ScreenManager : IDisposable
{
    private static event GLFWCallbacks.MonitorCallback? MonitorChanged;
    private readonly Dictionary<IntPtr, Monitor> _monitors = new();
    private readonly MonitorCreator _monitorCreator;

    public ScreenManager(MonitorCreator creator)
    {
        RenderSystem.AssertInInitPhase();
        _monitorCreator = creator;
        MonitorChanged += OnMonitorChanged;

        var monitors = GLFW.GetMonitors();
        foreach (var monitor in monitors)
        {
            _monitors.Add((IntPtr) monitor, _monitorCreator(monitor));
        }
    }

    private void OnMonitorChanged(NativeMonitor* monitor, ConnectedState state)
    {
        RenderSystem.AssertOnRenderThread();
        
        var key = (IntPtr) monitor;
        if (state == ConnectedState.Connected)
        {
            _monitors[key] = _monitorCreator(monitor);
        } else if (state == ConnectedState.Disconnected)
        {
            _monitors.Remove(key);
        }
    }

    public Monitor? GetMonitor(NativeMonitor* monitor)
    {
        RenderSystem.AssertInInitPhase();
        var key = (IntPtr) monitor;
        return _monitors.TryGetValue(key, out var result) ? result : null;
    }

    static ScreenManager()
    {
        GLFW.SetMonitorCallback(DelegateOnMonitorChange);
    }

    private static void DelegateOnMonitorChange(
        OpenTK.Windowing.GraphicsLibraryFramework.Monitor* monitor,
        ConnectedState state) => MonitorChanged?.Invoke(monitor, state);

    public Monitor? FindBestMonitor(Window window)
    {
        var monitor = GLFW.GetWindowMonitor(window.Handle);
        if (monitor != null) return GetMonitor(monitor);

        var i = window.X;
        throw new NotImplementedException();
    }

    private void Shutdown()
    {
        RenderSystem.AssertOnRenderThread();
        GLFW.SetMonitorCallback(null);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Shutdown();
    }
}