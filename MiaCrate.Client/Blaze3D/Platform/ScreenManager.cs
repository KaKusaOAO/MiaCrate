using System.Runtime.CompilerServices;
using MiaCrate.Client.Systems;
using SDL2;

namespace MiaCrate.Client.Platform;

public delegate Monitor MonitorCreator(int monitor);

public class ScreenManager : IDisposable
{
    private readonly Dictionary<int, Monitor> _monitors = new();
    private readonly MonitorCreator _monitorCreator;

    public ScreenManager(MonitorCreator creator)
    {
        RenderSystem.AssertInInitPhase();
        _monitorCreator = creator;
        RefreshMonitors();

        Window.ReceiveSDLEvent += HandleEvent;
    }

    private void HandleEvent(SDL.SDL_Event ev)
    {
        if (ev.type != SDL.SDL_EventType.SDL_DISPLAYEVENT) return;
        
        var dev = ev.display;
        var monitor = (int) dev.display;
        switch (dev.displayEvent)
        {
            case SDL.SDL_DisplayEventID.SDL_DISPLAYEVENT_CONNECTED:
                OnMonitorChanged(monitor, ConnectedState.Connected);
                break;
            case SDL.SDL_DisplayEventID.SDL_DISPLAYEVENT_DISCONNECTED:
                OnMonitorChanged(monitor, ConnectedState.Disconnected);
                break;
        }
    }

    private void OnMonitorChanged(int monitor, ConnectedState state)
    {
        RenderSystem.AssertOnRenderThread();
        
        if (state == ConnectedState.Connected)
        {
            _monitors[monitor] = _monitorCreator(monitor);
        } 
        else if (state == ConnectedState.Disconnected)
        {
            if (monitor + 1 == _monitors.Count)
            {
                _monitors.Remove(monitor);
                return;
            }
            
            RefreshMonitors();
        }
    }

    private void RefreshMonitors()
    {
        _monitors.Clear();
            
        var count = SDL.SDL_GetNumVideoDisplays();
        if (count < 1)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }
        
        for (var i = 0; i < count; i++)
        {
            _monitors.Add(i, _monitorCreator(i));
        }
    }

    public Monitor? GetMonitor(int monitor)
    {
        RenderSystem.AssertInInitPhase();
        return _monitors.TryGetValue(monitor, out var result) ? result : null;
    }

    static ScreenManager()
    {
        
    }

    public Monitor? FindBestMonitor(Window window)
    {
        var index = SDL.SDL_GetWindowDisplayIndex(window.Handle);
        if (index < 0)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }
        
        return GetMonitor(index);
    }

    private void Shutdown()
    {
        RenderSystem.AssertOnRenderThread();
        Window.ReceiveSDLEvent -= HandleEvent;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Shutdown();
    }
}

public enum ConnectedState
{
    Disconnected, Connected
}