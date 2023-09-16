using System.Collections.Concurrent;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client.Systems;

public static class RenderSystem
{
    private static readonly ConcurrentQueue<Action> _recordingQueue = new();
    private static readonly int[] _shaderTextures = new int[12];
    private static readonly float[] _shaderColor = new float[4];
    private static bool _isReplayingQueue;
    private static bool _isInInit;
    private static Thread? _gameThread;
    private static Thread? _renderThread;
    private static string? _apiDescription;
    private static ShaderInstance? _shader;
    private static int _maxSupportedTextureSize = -1;
    
    public static bool IsOnRenderThread => Thread.CurrentThread == _renderThread;
    public static bool IsOnRenderThreadOrInit => _isInInit || IsOnRenderThread;
    public static bool IsOnGameThread => true; // ?
    public static bool IsInInitPhase => true; // ?

    public static int MaxSupportedTextureSize
    {
        get
        {
            if (_maxSupportedTextureSize != -1) return _maxSupportedTextureSize;
            
            AssertOnRenderThreadOrInit();
            var i = GlStateManager.GetInteger(GetPName.MaxTextureSize);

            for (var j = Math.Max(32768, i); j >= 1024; j >>= 1)
            {
                GlStateManager.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, j, j, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
                var k = GlStateManager.GetTexLevelParameter(TextureTarget.Texture2D, 0,
                    GetTextureParameter.TextureWidth);
                if (k != 0)
                {
                    _maxSupportedTextureSize = j;
                    return j;
                }
            }

            _maxSupportedTextureSize = Math.Max(i, 1024);
            Logger.Info($"Failed to determine maximum texture size by probing, trying GL_MAX_TEXTURE_SIZE = {_maxSupportedTextureSize}");
            return _maxSupportedTextureSize;
        }
    }
    
    public static void EnsureOnRenderThreadOrInit(Action action)
    {
        if (!IsOnRenderThreadOrInit) RecordRenderCall(action);
        else action();
    }

    public static void EnsureOnRenderThread(Action action)
    {
        if (!IsOnRenderThread) RecordRenderCall(action);
        else action();
    }

    public static void EnsureOnGameThread(Action action)
    {
        if (!IsOnGameThread) RecordRenderCall(action);
        else action();
    }

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
    
    public static unsafe void FlipFrame(Window* handle)
    {
        PollEvents();
        ReplayQueue();
        GLFW.SwapBuffers(handle);
        PollEvents();
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

    public static void FinishInitialization()
    {
        _isInInit = false;
        if (_recordingQueue.Any()) ReplayQueue();
        if (_recordingQueue.Any()) throw new Exception("Recorded to render queue during initialization");
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

    public static void SetShaderTexture(int i, ResourceLocation location)
    {
        if (!IsOnRenderThread)
        {
            RecordRenderCall(() => CheckedSetShaderTexture(i, location));
        }
        else
        {
            CheckedSetShaderTexture(i, location);
        }
    }

    public static void CheckedSetShaderTexture(int i, ResourceLocation location)
    {
        if (i < 0 || i >= _shaderTextures.Length) return;
        var manager = Game.Instance.TextureManager;
        var texture = manager.GetTexture(location);
        _shaderTextures[i] = texture.Id;
    }
    
    public static void SetShaderTexture(int i, int textureId)
    {
        if (!IsOnRenderThread)
        {
            RecordRenderCall(() => CheckedSetShaderTexture(i, textureId));
        }
        else
        {
            CheckedSetShaderTexture(i, textureId);
        }
    }

    public static void CheckedSetShaderTexture(int i, int textureId)
    {
        if (i < 0 || i >= _shaderTextures.Length) return;
        _shaderTextures[i] = textureId;
    }

    public static int GetShaderTexture(int i)
    {
        AssertOnRenderThread();
        return i >= 0 && i < _shaderTextures.Length ? _shaderTextures[i] : 0;
    }

    public static void EnableDepthTest()
    {
        // Should this be render thread instead?
        AssertOnGameThreadOrInit();
        GlStateManager.EnableDepthTest();
    }

    public static void DepthFunc(DepthFunction func)
    {
        AssertOnRenderThread();
        GlStateManager.DepthFunc(func);
    }

    public static void DisableDepthTest()
    {
        AssertOnRenderThread(); // Parity?
        GlStateManager.DisableDepthTest();
    }

    public static void DisableBlend()
    {
        AssertOnRenderThread();
        GlStateManager.DisableBlend();
    }

    public static void EnableBlend()
    {
        AssertOnRenderThread();
        GlStateManager.EnableBlend();
    }

    public static void BlendFunc(BlendingFactorSrc sourceFactor, BlendingFactorDest destFactor)
    {
        AssertOnRenderThread();
        GlStateManager.BlendFunc(sourceFactor, destFactor);
    }

    public static void BlendFuncSeparate(
        BlendingFactorSrc sourceRgb, BlendingFactorDest destRgb,
        BlendingFactorSrc sourceAlpha, BlendingFactorDest destAlpha)
    {
        AssertOnRenderThread();
        GlStateManager.BlendFuncSeparate(sourceRgb, destRgb, sourceAlpha, destAlpha);
    }

    public static void DefaultBlendFunc() => BlendFuncSeparate(
        BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, 
        BlendingFactorSrc.One, BlendingFactorDest.Zero);

    public static void Uniform1(int location, int[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform1(location, buffer);
    }
    
    public static void Uniform1(int location, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform1(location, buffer);
    }

    public static void Uniform1(int location, int value)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform1(location, value);
    }
    
    public static void Uniform2(int location, int[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform2(location, buffer);
    }
    
    public static void Uniform2(int location, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform2(location, buffer);
    }
    
    public static void Uniform3(int location, int[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform3(location, buffer);
    }
    
    public static void Uniform3(int location, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform3(location, buffer);
    }
    
    public static void Uniform4(int location, int[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform4(location, buffer);
    }
    
    public static void Uniform4(int location, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.Uniform4(location, buffer);
    }
    
    public static void UniformMatrix2(int location, bool transpose, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.UniformMatrix2(location, transpose, buffer);
    }
    
    public static void UniformMatrix3(int location, bool transpose, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.UniformMatrix3(location, transpose, buffer);
    }
    
    public static void UniformMatrix4(int location, bool transpose, float[] buffer)
    {
        AssertOnRenderThread();
        GlStateManager.UniformMatrix4(location, transpose, buffer);
    }

    public static void BlendEquation(BlendEquationMode func)
    {
        AssertOnRenderThread();
        GlStateManager.BlendEquation(func);
    }

    public static void ActiveTexture(int textureUnit)
    {
        AssertOnGameThreadOrInit();
        GlStateManager.ActiveTexture(textureUnit);
    }

    public static void BindTexture(int texture) => 
        GlStateManager.BindTexture(texture);

    public static void BufferData(BufferTarget target, ReadOnlySpan<byte> span, BufferUsageHint usage)
    {
        AssertOnRenderThreadOrInit();
        GlStateManager.BufferData(target, span, usage);
    }
    
    public static void DrawElements(PrimitiveType mode, int count, DrawElementsType type)
    {
        AssertOnRenderThread();
        GlStateManager.DrawElements(mode, count, type, IntPtr.Zero);
    }
}