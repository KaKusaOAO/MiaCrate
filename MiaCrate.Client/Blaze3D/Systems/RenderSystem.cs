using System.Collections.Concurrent;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MiaCrate.Client.Systems;

public static class RenderSystem
{
    private static readonly ConcurrentQueue<Action> _recordingQueue = new();
    private static bool _isReplayingQueue;
    private static bool _isInInit;
    private static Thread? _gameThread;
    private static Thread? _renderThread;
    private static string? _apiDescription;
    private static ShaderInstance? _shader;

    public static bool IsOnRenderThread => Thread.CurrentThread == _renderThread;
    public static bool IsOnRenderThreadOrInit => _isInInit || IsOnRenderThread;
    public static bool IsOnGameThread => true; // ?
    public static bool IsInInitPhase => true; // ?

    public static void InitRenderThread()
    {
        if (_renderThread != null || _gameThread == Thread.CurrentThread)
            throw new Exception("Could not initialize render thread");
        
        _renderThread = Thread.CurrentThread;
    }

    public static void InitGameThread(bool bl)
    {
        var bl2 = _renderThread == Thread.CurrentThread;
        if (_gameThread != null || _renderThread == null || bl2 == bl)
            throw new Exception("Could not initialize tick thread");

        _gameThread = Thread.CurrentThread;
    }

    public static void AssertInInitPhase()
    {
        if (!IsInInitPhase) throw ConstructThreadException();
    }

    public static void AssertOnGameThread()
    {
        if (!IsOnGameThread) throw ConstructThreadException();
    }
    
    public static void AssertOnGameThreadOrInit()
    {
        if (!_isInInit && !IsOnGameThread) throw ConstructThreadException();
    }
    
    public static void AssertOnRenderThread()
    {
        if (!IsOnRenderThread) throw ConstructThreadException();
    }
    
    public static void AssertOnRenderThreadOrInit()
    {
        if (!_isInInit && !IsOnRenderThread) throw ConstructThreadException();
    }

    private static Exception ConstructThreadException() => new("RenderSystem called from wrong thread.");

    public static void RecordRenderCall(Action action) => _recordingQueue.Enqueue(action);

    private static void PollEvents()
    {
        GLFW.PollEvents();
    }

    public static bool IsFrozenAtPollEvents => throw new NotImplementedException();

    public static void ReplayQueue()
    {
        // What is this for?
        _isReplayingQueue = true;
        
        while (!_recordingQueue.IsEmpty)
        {
            if (_recordingQueue.TryDequeue(out var action)) action();
        }

        _isReplayingQueue = false;
    }
    
    public static void InitRenderer(int i, bool bl)
    {
        AssertInInitPhase();
        GLX.Init(i, bl);
        _apiDescription = GLX.OpenGlVersion;
    }

    public static void BeginInitialization()
    {
        _isInInit = true;
    }

    public static INanoTimeSource InitBackendSystem()
    {
        AssertInInitPhase();
        return INanoTimeSource.Create(GLX.InitGlfw());
    }

    public static string GetBackendDescription()
    {
        AssertInInitPhase();
        return $"{typeof(GL).Assembly}";
    }

    public static void SetShader(Func<ShaderInstance?> shader)
    {
        if (!IsOnRenderThread)
        {
            RecordRenderCall(() =>
            {
                _shader = shader();
            });
        }
        else
        {
            _shader = shader();
        }
    }
}