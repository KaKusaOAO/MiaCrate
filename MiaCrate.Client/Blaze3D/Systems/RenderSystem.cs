using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using MiaCrate.Client.Graphics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Shaders;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = OpenTK.Windowing.GraphicsLibraryFramework.Window;

namespace MiaCrate.Client.Systems;

public static class RenderSystem
{
    private static readonly ConcurrentQueue<Action> _recordingQueue = new();
    private static readonly Tesselator _renderThreadTesselator = new();
    private const int MinimumAtlasTextureSize = 1024;
    private static bool _isReplayingQueue;
    private static Thread? _gameThread;
    private static Thread? _renderThread;
    private static int _maxSupportedTextureSize = -1;
    private static bool _isInInit;
    private static double _lastDrawTime = double.MinValue;
    private static AutoStorageIndexBuffer _sharedSequential =
        new AutoStorageIndexBuffer(1, 1, (consumer, i) => consumer(i));
    private static AutoStorageIndexBuffer _sharedSequentialQuad =
        new AutoStorageIndexBuffer(4, 6, (consumer, i) =>
        {
            consumer(i + 0);
            consumer(i + 1);
            consumer(i + 2);
            consumer(i + 2);
            consumer(i + 3);
            consumer(i + 0);
        });
    private static AutoStorageIndexBuffer _sharedSequentialLines =
        new AutoStorageIndexBuffer(4, 6, (consumer, i) =>
        {
            consumer(i + 0);
            consumer(i + 1);
            consumer(i + 2);
            consumer(i + 3);
            consumer(i + 2);
            consumer(i + 1);
        });
    private static Matrix3 _inverseViewRotationMatrix = Matrix3.Zero;
    private static Matrix4 _projectionMatrix = Matrix4.Identity;
    private static Matrix4 _savedProjectionMatrix = Matrix4.Identity;
    private static IVertexSorting _vertexSorting = IVertexSorting.DistanceToOrigin;
    private static IVertexSorting _savedVertexSorting = IVertexSorting.DistanceToOrigin;
    private static Matrix4 _modelViewMatrix = Matrix4.Identity;
    private static Matrix4 _textureMatrix = Matrix4.Identity;
    private static readonly int[] _shaderTextures = new int[12];
    private static readonly float[] _shaderColor = {1, 1, 1, 1};
    private static float _shaderGlintAlpha = 1f;
    private static float _shaderFogEnd = 1f;
    private static readonly float[] _shaderFogColor = new float[4];
    private static FogShape _shaderFogShape = FogShape.Sphere;
    private static readonly Vector3[] _shaderLightDirections = new Vector3[2];
    private static float _shaderLineWidth = 1f;
    private static string _apiDescription = "Unknown";
    private static ShaderInstance? _shader;
    private static long _pollEventsWaitStart;
    private static bool _pollingEvents;

    public static Tesselator RenderThreadTesselator
    {
        get
        {
            AssertOnRenderThread();
            return _renderThreadTesselator;
        }
    }
    
    public static IVertexSorting VertexSorting
    {
        get
        {
            AssertOnRenderThread();
            return _vertexSorting;
        }
    }

    public static PoseStack ModelViewStack { get; } = new();

    public static Matrix4 ModelViewMatrix
    {
        get
        {
            AssertOnRenderThread();
            return _modelViewMatrix;
        }
    }
    
    public static Matrix4 ProjectionMatrix
    {
        get
        {
            AssertOnRenderThread();
            return _projectionMatrix;
        }
    }

    public static Matrix3 InverseViewRotationMatrix
    {
        get
        {
            AssertOnRenderThread();
            return _inverseViewRotationMatrix;
        }
    }

    public static float[] ShaderColor
    {
        get
        {
            AssertOnRenderThread();
            return _shaderColor;
        }
    }
    
    public static ShaderInstance? Shader
    {
        get
        {
            AssertOnRenderThread();
            return _shader;
        }
    }
    
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

            for (var j = Math.Max(32768, i); j >= MinimumAtlasTextureSize; j >>= 1)
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

            _maxSupportedTextureSize = Math.Max(i, MinimumAtlasTextureSize);
            Logger.Info($"Failed to determine maximum texture size by probing, trying GL_MAX_TEXTURE_SIZE = {_maxSupportedTextureSize}");
            return _maxSupportedTextureSize;
        }
    }
    
    public static void EnsureInInitPhase(Action action)
    {
        if (!IsInInitPhase) RecordRenderCall(action);
        else action();
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

    public static void Clear(ClearBufferMask mask, bool clearError)
    {
        AssertOnGameThreadOrInit();
        GlStateManager.Clear(mask, clearError);
    }

    public static void EnableCull()
    {
        AssertOnRenderThread();
        GlStateManager.EnableCull();
    }

    public static void DepthMask(bool flag)
    {
        AssertOnRenderThread();
        GlStateManager.DepthMask(flag);
    }
    
    public static void ColorMask(bool red, bool green, bool blue, bool alpha)
    {
        AssertOnRenderThread();
        GlStateManager.ColorMask(red, green, blue, alpha);
    }

    public static void SetShaderColor(float r, float g, float b, float a) => 
        EnsureOnRenderThread(() => InternalSetShaderColor(r, g, b, a));

    private static void InternalSetShaderColor(float r, float g, float b, float a)
    {
        _shaderColor[0] = r;
        _shaderColor[1] = g;
        _shaderColor[2] = b;
        _shaderColor[3] = a;
    }

    public static AutoStorageIndexBuffer GetSequentialBuffer(VertexFormat.Mode mode)
    {
        AssertOnRenderThread();
        if (mode == VertexFormat.Mode.Quads) return _sharedSequentialQuad;
        if (mode == VertexFormat.Mode.Lines) return _sharedSequentialLines;
        return _sharedSequential;
    }

    public static void DisableCull()
    {
        AssertOnRenderThread();
        GlStateManager.DisableCull();
    }

    public static void SetProjectionMatrix(Matrix4 matrix, IVertexSorting vertexSorting)
    {
        EnsureOnRenderThread(() =>
        {
            _projectionMatrix = matrix;
            _vertexSorting = vertexSorting;
        });
    }

    public static void LimitDisplayFps(int fps)
    {
        var d = _lastDrawTime + 1.0 / fps;
        
        double e;
        for (e = GLFW.GetTime(); e < d; e = GLFW.GetTime())
        {
            GLFW.WaitEventsTimeout(d - e);
        }

        _lastDrawTime = e;
    }

    public static void Viewport(int x, int y, int width, int height)
    {
        AssertOnGameThreadOrInit();
        GlStateManager.Viewport(x, y, width, height);
    }

    public static void ApplyModelViewMatrix()
    {
        var matrix = ModelViewStack.Last.Pose;
        EnsureOnRenderThread(() => _modelViewMatrix = matrix);
    }

    public static void BackupProjectionMatrix()
    {
        EnsureOnRenderThread(() =>
        {
            _savedProjectionMatrix = _projectionMatrix;
            _savedVertexSorting = _vertexSorting;
        });
    }
    
    public static void RestoreProjectionMatrix()
    {
        EnsureOnRenderThread(() =>
        {
            _projectionMatrix = _savedProjectionMatrix;
            _vertexSorting = _savedVertexSorting;
        });
    }

    public static void TexParameter(TextureTarget target, TextureParameterName pName, int value) => 
        GlStateManager.TexParameter(target, pName, value);

    public static void TexParameter(TextureTarget target, TextureParameterName pName, float value) => 
        GlStateManager.TexParameter(target, pName, value);
    
    /// <summary>
    /// For quick check, this applies to 10241 (<see cref="TextureParameterName.TextureMinFilter"/>).
    /// </summary>
    public static void TexMinFilter(TextureTarget target, TextureMinFilter filter) =>
        TexParameter(target, TextureParameterName.TextureMinFilter, (int) filter);

    /// <summary>
    /// For quick check, this applies to 10240 (<see cref="TextureParameterName.TextureMagFilter"/>).
    /// </summary>
    public static void TexMagFilter(TextureTarget target, TextureMagFilter filter) =>
        TexParameter(target, TextureParameterName.TextureMagFilter, (int) filter);
     
    public static void PolygonOffset(float factor, float units)
    {
        AssertOnRenderThread();
        GlStateManager.PolygonOffset(factor, units);
    }

    public static void EnablePolygonOffset()
    {
        AssertOnRenderThread();
        GlStateManager.EnablePolygonOffset();
    }
    
    public static void DisablePolygonOffset()
    {
        AssertOnRenderThread();
        GlStateManager.DisablePolygonOffset();
    }

    public static void SetupOverlayColor(Func<int> textureId, int size)
    {
        // size is unused??
        AssertOnRenderThread();
        SetShaderTexture(1, textureId());
    }
    
    public static void TeardownOverlayColor()
    {
        AssertOnRenderThread();
        SetShaderTexture(1, 0);
    }

    #region => AutoStorageIndexBuffer
    public sealed class AutoStorageIndexBuffer
    {
        private readonly int _vertexStride;
        private readonly int _indexStride;
        private readonly IndexGenerator _generator;
        
        private int _name;
        private int _indexCount;
        
        public VertexFormat.IndexType Type { get; private set; }

        public AutoStorageIndexBuffer(int vertexStride, int indexStride, IndexGenerator generator)
        {
            Type = VertexFormat.IndexType.Short;
            _vertexStride = vertexStride;
            _indexStride = indexStride;
            _generator = generator;
        }

        public bool HasStorage(int count) => count <= _indexCount;

        public void Bind(int count)
        {
            if (_name == 0)
            {
                _name = GlStateManager.GenBuffers();
                GlStateManager.ObjectLabel(ObjectLabelIdentifier.Buffer, _name, $"{nameof(AutoStorageIndexBuffer)} #{_name}");
            }

            GlStateManager.BindBuffer(BufferTarget.ElementArrayBuffer, _name);
            EnsureStorage(count);
        }

        private void EnsureStorage(int count)
        {
            if (HasStorage(count)) return;

            count = Util.RoundToward(count * 2, _indexStride);
            Logger.Verbose($"Growing IndexBuffer: Old limit {_indexCount}, new limit {count}");
            var indexType = VertexFormat.IndexType.Least(count);

            var j = Util.RoundToward(count * indexType.Bytes, 4);
            GlStateManager.BufferData(BufferTarget.ElementArrayBuffer, j, BufferUsageHint.DynamicDraw);

            var buffer = GlStateManager.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.WriteOnly);
            if (buffer == 0)
            {
                throw new Exception("Failed to map GL buffer");
            }

            Type = indexType;
            var tracker = new PointerTrackingIntConsumer(buffer);
            var consumer = IntConsumer(tracker);

            for (var k = 0; k < count; k += _indexStride)
            {
                _generator(consumer, k * _vertexStride / _indexStride);
            }

            GlStateManager.UnmapBuffer(BufferTarget.ElementArrayBuffer);
            _indexCount = count;
        }

        private Action<int> IntConsumer(PointerTrackingIntConsumer tracker)
        {
            if (Type == VertexFormat.IndexType.Short)
            {
                return tracker.WriteShort;
            }

            return tracker.WriteInt;
        }

        private class PointerTrackingIntConsumer
        {
            private IntPtr _buffer;

            public PointerTrackingIntConsumer(IntPtr buffer)
            {
                _buffer = buffer;
            }

            public void WriteShort(int i)
            {
                Marshal.WriteInt16(_buffer, (short) i);
                _buffer += sizeof(short);
            }

            public void WriteInt(int i)
            {
                Marshal.WriteInt32(_buffer, i);
                _buffer += sizeof(int);
            }
        }

        public delegate void IndexGenerator(Action<int> consumer, int i);
    }
    #endregion
}