using MiaCrate.Client.Multiplayer;
using MiaCrate.Client.Pipeline;
using MiaCrate.Client.Resources;
using MiaCrate.Client.Systems;
using MiaCrate.Core;
using MiaCrate.Resources;
using MiaCrate.World.Blocks;
using MiaCrate.World.Phys;
using Mochi.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class LevelRenderer : IResourceManagerReloadListener, IDisposable
{
    public const int SectionSize = SectionPos.SectionSize;
    public const int HalfSectionSize = SectionSize / 2;
    private const float SkyDiscRadius = 512;
    private const int MinFogDistance = 32;
    private const int RainRadius = RainDiameter / 2;
    private const int RainDiameter = 21;
    private const int TransparentSortCount = 15;

    private static ResourceLocation MoonLocation { get; } = new("textures/environment/moon_phases.png");
    private static ResourceLocation SunLocation { get; } = new("textures/environment/sun.png");
    private static ResourceLocation CloudsLocation { get; } = new("textures/environment/clouds.png");
    private static ResourceLocation EndSkyLocation { get; } = new("textures/environment/end_sky.png");
    private static ResourceLocation ForceFieldLocation { get; } = new("textures/misc/forcefield.png");
    private static ResourceLocation RainLocation { get; } = new("textures/environment/rain.png");
    private static ResourceLocation SnowLocation { get; } = new("textures/environment/snow.png");
    
    private readonly Game _game;
    private readonly EntityRenderDispatcher _entityRenderDispatcher;
    private readonly BlockEntityRenderDispatcher _blockEntityRenderDispatcher;
    private readonly RenderBuffers _renderBuffers;

    private ClientLevel? _level;
    private readonly HashSet<BlockEntity> _globalBlockEntities = new();
    private VertexBuffer? _starBuffer;
    private VertexBuffer? _skyBuffer;
    private VertexBuffer? _darkBuffer;
    private VertexBuffer? _cloudBuffer;
    private bool _generateClouds = true;

    private int _ticks;
    private readonly Dictionary<BlockPos, ISoundInstance> _playingRecords = new();

    private PostChain? _entityEffect;
    private PostChain? _transparencyChain;
    private RenderTarget? _entityTarget;
    private RenderTarget? _translucentTarget;
    private RenderTarget? _itemEntityTarget;
    private RenderTarget? _particlesTarget;
    private RenderTarget? _weatherTarget;
    private RenderTarget? _cloudsTarget;

    private int _lastCameraSectionX = int.MinValue;
    private int _lastCameraSectionY = int.MinValue;
    private int _lastCameraSectionZ = int.MinValue;
    
    private double _prevCamX = double.MinValue;
    private double _prevCamY = double.MinValue;
    private double _prevCamZ = double.MinValue;
    private double _prevCamRotX = double.MinValue;
    private double _prevCamRotY = double.MinValue;

    private int _prevCloudX = int.MinValue;
    private int _prevCloudY = int.MinValue;
    private int _prevCloudZ = int.MinValue;

    private Vec3 _prevCloudColor = Vec3.Zero;

    private int _lastViewDistance = -1;
    private int _renderedEntities;
    private int _culledEntities;
    private Frustum _cullingFrustum;
    
    private bool _captureFrustum;
    private Frustum? _capturedFrustum;
    private readonly Vector4[] _frustumPoints = new Vector4[8];
    private Vector3d _frustumPos = Vector3d.Zero;

    private double _xTransparentOld;
    private double _yTransparentOld;
    private double _zTransparentOld;

    private int _rainSoundTime;
    private readonly float[] _rainSizeX = new float[1 << RainRadius];
    private readonly float[] _rainSizeZ = new float[1 << RainRadius];
    
    public LevelRenderer(Game game, EntityRenderDispatcher entityRenderDispatcher,
        BlockEntityRenderDispatcher blockEntityRenderDispatcher, RenderBuffers renderBuffers)
    {
        _game = game;
        _entityRenderDispatcher = entityRenderDispatcher;
        _blockEntityRenderDispatcher = blockEntityRenderDispatcher;
        _renderBuffers = renderBuffers;

        for (var i = 0; i < SectionSize * 2; i++)
        {
            for (var j = 0; j < SectionSize * 2; j++)
            {
                var f = j - SectionSize;
                var g = i - SectionSize;
                var h = MathF.Sqrt(f * f + g * g);

                _rainSizeX[i << (RainRadius / 2) | j] = -g / h;
                _rainSizeZ[i << (RainRadius / 2) | j] = f / h;
            }
        }

        CreateStars();
        CreateLightSky();
        CreateDarkSky();
    }

    private void CreateDarkSky()
    {
        var tesselator = Tesselator.Instance;
        var builder = tesselator.Builder;
        
        _darkBuffer?.Dispose();
        _darkBuffer = new VertexBuffer(BufferUsageHint.StaticDraw);

        var rendered = BuildSkyDisc(builder, -16f);
        _darkBuffer.Bind();
        _darkBuffer.Upload(rendered);
        VertexBuffer.Unbind();
    }

    private void CreateLightSky()
    {
        var tesselator = Tesselator.Instance;
        var builder = tesselator.Builder;
        
        _skyBuffer?.Dispose();
        _skyBuffer = new VertexBuffer(BufferUsageHint.StaticDraw);

        var rendered = BuildSkyDisc(builder, 16f);
        _skyBuffer.Bind();
        _skyBuffer.Upload(rendered);
        VertexBuffer.Unbind();
    }

    private BufferBuilder.RenderedBuffer BuildSkyDisc(BufferBuilder builder, float f)
    {
        var g = MathF.Sign(f) * 512f;
        var h = 512f;
        
        RenderSystem.SetShader(() => GameRenderer.PositionShader);
        builder.Begin(VertexFormat.Mode.TriangleFan, DefaultVertexFormat.Position);
        builder.Vertex(0, f, 0).EndVertex();

        for (var i = -180; i <= 180; i += 45)
        {
            builder
                .Vertex(g * MathF.Cos(i * (float) Mth.DegToRad), f, 512f * MathF.Sin(i * (float) Mth.DegToRad))
                .EndVertex();
        }

        return builder.End();
    }

    private void CreateStars()
    {
        var tesselator = Tesselator.Instance;
        var builder = tesselator.Builder;
        RenderSystem.SetShader(() => GameRenderer.PositionShader);
        
        _starBuffer?.Dispose();
        _starBuffer = new VertexBuffer(BufferUsageHint.StaticDraw);

        var rendered = DrawStars(builder);
        _starBuffer.Bind();
        _starBuffer.Upload(rendered);
        VertexBuffer.Unbind();
    }

    private BufferBuilder.RenderedBuffer DrawStars(BufferBuilder builder)
    {
        var rand = IRandomSource.Create(10842);
        builder.Begin(VertexFormat.Mode.Quads, DefaultVertexFormat.Position);

        for (var i = 0; i < 1500; i++)
        {
            var d = rand.NextSingle() * 2 - 1;
            var e = rand.NextSingle() * 2 - 1;
            var f = rand.NextSingle() * 2 - 1;
            var g = 0.15f + rand.NextSingle() * 0.1f;
            var h = d * d + e * e + f * f;

            if (h is < 1 and > 0.01f)
            {
                h = 1 / MathF.Sqrt(h);
                d *= h;
                e *= h;
                f *= h;

                var j = d * 100;
                var k = e * 100;
                var l = f * 100;

                var m = MathF.Atan2(d, f);
                var n = MathF.Sin(m);
                var o = MathF.Cos(m);

                var p = MathF.Atan2(MathF.Sqrt(d * d + f * f), e);
                var q = MathF.Sin(p);
                var r = MathF.Cos(p);

                var s = rand.NextSingle() * float.Pi * 2;
                var t = MathF.Sin(s);
                var u = MathF.Cos(s);

                for (var v = 0; v < 4; v++)
                {
                    var w = 0f;
                    var x = ((v & 2) - 1) * g;
                    var y = (((v + 1) & 2) - 1) * g;
                    var z = 0f;

                    var aa = x * u - y * t;
                    var ab = y * u + x * t;
                    var ad = aa * q + 0 * r; // ?
                    var ae = 0 * q - aa * r; // ?
                    var af = ae * n - ab * o;
                    var ah = ab * n + ae * o;
                    
                    builder.Vertex(j + af, k + ad, l + ah).EndVertex();
                }
            }
        }

        return builder.End();
    }

    public void OnResourceManagerReload(IResourceManager manager)
    {
        InitOutline();
    }

    public void InitOutline()
    {
        _entityEffect?.Dispose();

        var location = new ResourceLocation("shaders/post/entity_outline.json");

        try
        {
            _entityEffect = new PostChain(_game.TextureManager, _game.ResourceManager, _game.MainRenderTarget,
                location);
            _entityEffect.Resize(_game.Window.Width, _game.Window.Height);
            _entityTarget = _entityEffect.GetTempTarget("final");
        }
        catch (Exception ex)
        {
            Logger.Warn($"Failed to load shader: {location}");
            Logger.Warn(ex);

            _entityEffect = null;
            _entityTarget = null;
        }
    }

    public void SetLevel(ClientLevel? level)
    {
        _lastCameraSectionX = int.MinValue;
        _lastCameraSectionY = int.MinValue;
        _lastCameraSectionZ = int.MinValue;
        _entityRenderDispatcher.SetLevel(level);
        _level = level;

        if (level != null)
        {
            AllChanged();
            return;
        }

        Util.LogFoobar();
    }

    public void AllChanged()
    {
        if (_level == null) return;

        GraphicsChanged();

        Util.LogFoobar();
        _generateClouds = true;
    }

    public void GraphicsChanged()
    {
        Util.LogFoobar();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void NeedsUpdate()
    {
        Util.LogFoobar();
    }
}