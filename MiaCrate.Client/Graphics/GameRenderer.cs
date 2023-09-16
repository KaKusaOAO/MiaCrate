using System.Resources;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Data;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class GameRenderer : IDisposable
{
    private readonly Game _game;
    private readonly ItemInHandRenderer _itemInHandRenderer;
    private readonly IResourceManager _resourceManager;
    private readonly RenderBuffers _renderBuffers;

    private static class ShaderNames
    {
        public const string BlitScreen = "blit_screen";
        private const string RenderTypePrefix = "rendertype";
        public const string RenderTypeGui = $"{RenderTypePrefix}_gui";
        public const string RenderTypeGuiOverlay = $"{RenderTypeGui}_overlay";
        public const string RenderTypeText = $"{RenderTypePrefix}_text";

        public const string Position = "position";
        private const string Color = "color";
        private const string Tex = "tex";
        public const string PositionColor = $"{Position}_{Color}";
        public const string PositionColorTex = $"{Position}_{Color}_{Tex}";
        public const string PositionTex = $"{Position}_{Tex}";
        public const string PositionTexColor = $"{Position}_{Tex}_{Color}";
    }
    
    private readonly Dictionary<string, ShaderInstance> _shaders = new();
    private PostChain? _postEffect;
    private ShaderInstance? _blitShader;

    public static ShaderInstance? RenderTypeTextShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiOverlayShader { get; private set; }
    public static ShaderInstance? PositionShader { get; private set; }
    public static ShaderInstance? PositionColorShader { get; private set; }
    public static ShaderInstance? PositionColorTexShader { get; private set; }
    public static ShaderInstance? PositionTexShader { get; private set; }
    public static ShaderInstance? PositionTexColorShader { get; private set; }

    public GameRenderer(Game game, ItemInHandRenderer itemInHandRenderer, IResourceManager resourceManager,
        RenderBuffers renderBuffers)
    {
        _game = game;
        _itemInHandRenderer = itemInHandRenderer;
        _resourceManager = resourceManager;
        _renderBuffers = renderBuffers;
    }
    
    private void LoadEffect(ResourceLocation location)
    {
        // _postEffect
    }
    
    private void ReloadShaders(IResourceProvider provider)
    {
        RenderSystem.AssertOnRenderThread();
        var list = new List<Program>();
        list.AddRange(ProgramType.Fragment.Programs.Values);
        list.AddRange(ProgramType.Vertex.Programs.Values);
        
        foreach (var program in list)
        {
            program.Dispose();
        }

        var list2 = new List<(ShaderInstance, Action<ShaderInstance>)>();

        try
        {
            list2.Add((new ShaderInstance(provider, ShaderNames.Position, DefaultVertexFormat.Position), 
                s => PositionShader = s));
            list2.Add((new ShaderInstance(provider, ShaderNames.PositionTex, DefaultVertexFormat.PositionTex), 
                s => PositionTexShader = s));
            list2.Add((new ShaderInstance(provider, ShaderNames.RenderTypeGui, DefaultVertexFormat.PositionColor), 
                s => RenderTypeGuiShader = s));
            list2.Add((new ShaderInstance(provider, ShaderNames.RenderTypeGuiOverlay, DefaultVertexFormat.PositionColor), 
                s => RenderTypeGuiOverlayShader = s));
        }
        catch (Exception ex)
        {
            foreach (var (shaderInstance, _) in list2)
            {
                shaderInstance.Dispose();
            }

            throw new Exception("Could not reload shaders", ex);
        }
        
        ShutdownShaders();
        foreach (var (shaderInstance, action) in list2)
        {
            _shaders[shaderInstance.Name] = shaderInstance;
            action(shaderInstance);
        }
    }

    private void ShutdownShaders()
    {
        RenderSystem.AssertOnRenderThread();
        foreach (var shader in _shaders.Values)
        {
            shader.Dispose();
        }
        
        _shaders.Clear();
    }

    public void PreloadUiShader(IResourceProvider provider)
    {
        if (_blitShader != null)
            throw new Exception("Blit shader already preloaded");

        try
        {
            _blitShader = new ShaderInstance(provider, ShaderNames.BlitScreen, DefaultVertexFormat.BlitScreen);
        }
        catch (Exception ex)
        {
            throw new Exception("Could not preload blit shader", ex);
        }

        RenderTypeGuiShader = PreloadShader(provider, ShaderNames.RenderTypeGui, DefaultVertexFormat.PositionColor);
        RenderTypeGuiOverlayShader =
            PreloadShader(provider, ShaderNames.RenderTypeGuiOverlay, DefaultVertexFormat.PositionColor);
        PositionShader = PreloadShader(provider, ShaderNames.Position, DefaultVertexFormat.Position);
        PositionColorShader = PreloadShader(provider, ShaderNames.PositionColor, DefaultVertexFormat.PositionColor);
        PositionColorTexShader =
            PreloadShader(provider, ShaderNames.PositionColorTex, DefaultVertexFormat.PositionColorTex);
        PositionTexShader = PreloadShader(provider, ShaderNames.PositionTex, DefaultVertexFormat.PositionTex);
        PositionTexColorShader =
            PreloadShader(provider, ShaderNames.PositionTexColor, DefaultVertexFormat.PositionTexColor);
        RenderTypeTextShader = PreloadShader(provider, ShaderNames.RenderTypeText,
            DefaultVertexFormat.PositionColorTexLightmap);
    }

    private ShaderInstance PreloadShader(IResourceProvider provider, string name, VertexFormat format)
    {
        try
        {
            var shader = new ShaderInstance(provider, name, format);
            _shaders[name] = shader;
            return shader;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not preload shader {name}", ex);
        }
    }

    public IPreparableReloadListener CreateReloadListener() => new ShaderLoader(this);
    
    private class ShaderLoader : SimplePreparableReloadListener<ResourceCache>
    {
        private readonly GameRenderer _renderer;

        public ShaderLoader(GameRenderer renderer)
        {
            _renderer = renderer;
        }

        protected override ResourceCache Prepare(IResourceManager manager, IProfilerFiller profiler)
        {
            var resources = manager.ListResources("shaders", location =>
            {
                var path = location.Path;
                return path.EndsWith(".json") || 
                       path.EndsWith(ProgramType.Fragment.Extension) ||
                       path.EndsWith(ProgramType.Vertex.Extension) || 
                       path.EndsWith(".glsl");
            });

            var cache = new Dictionary<ResourceLocation, Resource>();
            foreach (var (location, resource) in resources)
            {
                try
                {
                    using var stream = resource.Open();
                    var memory = new MemoryStream();
                    stream.CopyTo(memory);
                    var arr = memory.GetBuffer();
                    cache.Add(location, new Resource(resource.Source, () => new MemoryStream(arr)));
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Failed to read resource {location}");
                    Logger.Warn(ex);
                }
            }

            return new ResourceCache(manager, cache);
        }

        protected override void Apply(ResourceCache cache, IResourceManager manager, IProfilerFiller profiler)
        {
            _renderer.ReloadShaders(cache);
            _renderer._postEffect?.Dispose();
            _renderer._postEffect = null;
        }

        public override string Name => "Shader Loader";
    }

    public record ResourceCache(IResourceProvider Original, Dictionary<ResourceLocation, Resource> Cache) : IResourceProvider
    {
        public IOptional<Resource> GetResource(ResourceLocation location)
        {
            return Cache.TryGetValue(location, out var resource)
                ? Optional.Of(resource)
                : Original.GetResource(location);
        }
    }

    public void Dispose()
    {
        _postEffect?.Dispose();
    }
}