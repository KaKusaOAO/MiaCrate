using MiaCrate.Client.Systems;
using SDL2;

namespace MiaCrate.Client.Platform;

public class Monitor
{
    public int Index { get; }
    private readonly List<SDL.SDL_DisplayMode> _videoModes = new();
    private int _x;
    private int _y;
    public SDL.SDL_DisplayMode CurrentMode { get; private set; }

    public int X => _x;
    public int Y => _y;

    public Monitor(int index)
    {
        Index = index;
        RefreshVideoModes();
    }

    public static MonitorCreator Create => v => new Monitor(v);

    public void RefreshVideoModes()
    {
        RenderSystem.AssertInInitPhase();
        _videoModes.Clear();

        var count = SDL.SDL_GetNumDisplayModes(Index);
        if (count < 1)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }

        for (var i = 0; i < count; i++)
        {
            if (SDL.SDL_GetDisplayMode(Index, i, out var mode) != 0)
            {
                var err = SDL.SDL_GetError();
                throw new InvalidOperationException($"SDL error: {err}");
            }

            var format = mode.format;
            if (format == SDL.SDL_PIXELFORMAT_RGB24 || format == SDL.SDL_PIXELFORMAT_BGR24)
            {
                _videoModes.Add(mode);
            } 
            else if ((SDL.SDL_PackedLayout) SDL.SDL_PIXELLAYOUT(format) == SDL.SDL_PackedLayout.SDL_PACKEDLAYOUT_8888)
            {
                _videoModes.Add(mode);
            }
        }

        if (SDL.SDL_GetDisplayBounds(Index, out var rect) != 0)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }

        _x = rect.x;
        _y = rect.y;

        if (SDL.SDL_GetCurrentDisplayMode(0, out var currMode) != 0)
        {
            var err = SDL.SDL_GetError();
            throw new InvalidOperationException($"SDL error: {err}");
        }

        CurrentMode = currMode;
    }

    public SDL.SDL_DisplayMode GetPreferredVideoMode(SDL.SDL_DisplayMode? videoMode)
    {
        RenderSystem.AssertInInitPhase();
        if (videoMode.HasValue)
        {
            var mode = videoMode.Value;
            foreach (var m in _videoModes)
            {
                if (m.Equals(mode)) return m;
            }
        }

        return CurrentMode;
    }

    public int GetVideoModeIndex(SDL.SDL_DisplayMode videoMode)
    {
        RenderSystem.AssertInInitPhase();
        return _videoModes.IndexOf(videoMode);
    }

    public SDL.SDL_DisplayMode GetMode(int index) => _videoModes[index];

    public int GetModeCount() => _videoModes.Count;
}