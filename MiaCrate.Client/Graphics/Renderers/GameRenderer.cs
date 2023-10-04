using System.Collections.Concurrent;
using System.Diagnostics;
using MiaCrate.Client.Platform;
using MiaCrate.Client.Shaders;
using MiaCrate.Client.Systems;
using MiaCrate.Client.UI;
using MiaCrate.Resources;
using Mochi.Utils;
using OpenTK.Mathematics;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public class GameRenderer : IDisposable
{
    private readonly Game _game;
    private readonly ItemInHandRenderer _itemInHandRenderer;
    private readonly IResourceManager _resourceManager;
    private readonly RenderBuffers _renderBuffers;

    #region => Shader name constant definition
    private static class ShaderNames
    {
        public const string Particle = "particle";
        public const string Position = "position";
        private const string Color = "color";
        private const string Tex = "tex";
        private const string Lightmap = "lightmap";
        private const string Normal = "normal";

        public const string PositionColor = $"{Position}_{Color}";
        public const string PositionColorLightmap = $"{PositionColor}_{Lightmap}";
        public const string PositionColorTex = $"{PositionColor}_{Tex}";
        public const string PositionColorTexLightmap = $"{PositionColorTex}_{Lightmap}";

        public const string PositionTex = $"{Position}_{Tex}";
        public const string PositionTexColor = $"{PositionTex}_{Color}";
        public const string PositionTexColorNormal = $"{PositionTexColor}_{Normal}";
        public const string PositionTexLightmapColor = $"{PositionTex}_{Lightmap}_{Color}";

        private const string RenderTypePrefix = "rendertype";
        private const string Cutout = "cutout";
        private const string Cull = "cull";
        private const string NoCull = $"no_{Cull}";
        private const string CutoutNoCull = $"{Cutout}_{NoCull}";
        private const string Solid = "solid";

        public const string RenderTypeSolid = $"{RenderTypePrefix}_{Solid}";
        public const string RenderTypeCutout = $"{RenderTypePrefix}_{Cutout}";
        public const string RenderTypeCutoutMipped = $"{RenderTypeCutout}_mipped";
        public const string RenderTypeTranslucent = $"{RenderTypePrefix}_translucent";
        public const string RenderTypeTranslucentMovingBlock = $"{RenderTypeTranslucent}_moving_block";
        public const string RenderTypeTranslucentNoCrumbling = $"{RenderTypeTranslucent}_no_crumbling";
        public const string RenderTypeArmorCutoutNoCull = $"{RenderTypePrefix}_armor_{CutoutNoCull}";

        private const string RenderTypeEntity = $"{RenderTypePrefix}_entity";
        public const string RenderTypeEntitySolid = $"{RenderTypeEntity}_{Solid}";
        public const string RenderTypeEntityCutout = $"{RenderTypeEntity}_{Cutout}";
        public const string RenderTypeEntityCutoutNoCull = $"{RenderTypeEntity}_{CutoutNoCull}";
        public const string RenderTypeEntityCutoutNoCullZOffset = $"{RenderTypeEntityCutoutNoCull}_z_offset";

        public const string RenderTypeEntityTranslucent = $"{RenderTypeEntity}_translucent";
        public const string RenderTypeEntityTranslucentCull = $"{RenderTypeEntityTranslucent}_{Cull}";
        public const string RenderTypeItemEntityTranslucentCull = $"{RenderTypePrefix}_item_entity_translucent_{Cull}";
        public const string RenderTypeEntityTranslucentEmissive = $"{RenderTypeEntityTranslucent}_emissive";
        public const string RenderTypeEntitySmoothCutout = $"{RenderTypeEntity}_smooth_{Cutout}";
        public const string RenderTypeBeaconBeam = $"{RenderTypePrefix}_beacon_beam";
        public const string RenderTypeEntityDecal = $"{RenderTypeEntity}_decal";
        public const string RenderTypeEntityNoOutline = $"{RenderTypeEntity}_no_outline";
        public const string RenderTypeEntityShadow = $"{RenderTypeEntity}_shadow";
        public const string RenderTypeEntityAlpha = $"{RenderTypeEntity}_alpha";

        public const string RenderTypeEyes = $"{RenderTypePrefix}_eyes";
        public const string RenderTypeEnergySwirl = $"{RenderTypePrefix}_energy_swirl";
        public const string RenderTypeLeash = $"{RenderTypePrefix}_leash";
        public const string RenderTypeWaterMask = $"{RenderTypePrefix}_water_mask";
        public const string RenderTypeOutline = $"{RenderTypePrefix}_outline";

        private const string Glint = "glint";
        public const string RenderTypeGlint = $"{RenderTypePrefix}_{Glint}";
        public const string RenderTypeGlintDirect = $"{RenderTypeGlint}_direct";
        public const string RenderTypeGlintTranslucent = $"{RenderTypeGlint}_translucent";
        public const string RenderTypeArmorGlint = $"{RenderTypePrefix}_armor_{Glint}";
        public const string RenderTypeArmorEntityGlint = $"{RenderTypePrefix}_armor_entity_{Glint}";
        public const string RenderTypeEntityGlint = $"{RenderTypeEntity}_{Glint}";
        public const string RenderTypeEntityGlintDirect = $"{RenderTypeEntityGlint}_direct";

        public const string RenderTypeText = $"{RenderTypePrefix}_text";
        public const string RenderTypeTextBackground = $"{RenderTypeText}_background";
        public const string RenderTypeTextIntensity = $"{RenderTypeText}_intensity";

        private const string SeeThrough = "see_through";
        public const string RenderTypeTextSeeThrough = $"{RenderTypeText}_{SeeThrough}";
        public const string RenderTypeTextBackgroundSeeThrough = $"{RenderTypeTextBackground}_{SeeThrough}";
        public const string RenderTypeTextIntensitySeeThrough = $"{RenderTypeTextIntensity}_{SeeThrough}";

        public const string RenderTypeLightning = $"{RenderTypePrefix}_lightning";
        public const string RenderTypeTripwire = $"{RenderTypePrefix}_tripwire";
        public const string RenderTypeEndPortal = $"{RenderTypePrefix}_end_portal";
        public const string RenderTypeEndGateway = $"{RenderTypePrefix}_end_gateway";
        public const string RenderTypeLines = $"{RenderTypePrefix}_lines";
        public const string RenderTypeCrumbling = $"{RenderTypePrefix}_crumbling";
        public const string RenderTypeGui = $"{RenderTypePrefix}_gui";
        public const string RenderTypeGuiOverlay = $"{RenderTypeGui}_overlay";
        public const string RenderTypeGuiTextHighlight = $"{RenderTypeGui}_text_highlight";
        public const string RenderTypeGuiGhostRecipeOverlay = $"{RenderTypeGui}_ghost_recipe_overlay";

        public const string BlitScreen = "blit_screen";
    }
    #endregion

    private readonly Dictionary<string, ShaderInstance> _shaders = new();
    private PostChain? _postEffect;
    private ShaderInstance? _blitShader;
    public ShaderInstance BlitShader => _blitShader!;

    #region => All shader definitions
    #region => Basic shader definitions
    public static ShaderInstance? ParticleShader { get; private set; }
    public static ShaderInstance? PositionShader { get; private set; }
    public static ShaderInstance? PositionColorShader { get; private set; }
    public static ShaderInstance? PositionColorLightmapShader { get; private set; }
    public static ShaderInstance? PositionColorTexShader { get; private set; }
    public static ShaderInstance? PositionColorTexLightmapShader { get; private set; }
    public static ShaderInstance? PositionTexShader { get; private set; }
    public static ShaderInstance? PositionTexColorShader { get; private set; }
    public static ShaderInstance? PositionTexColorNormalShader { get; private set; }
    public static ShaderInstance? PositionTexLightmapColorShader { get; private set; }
    #endregion

    #region => Block shader definitions
    public static ShaderInstance? RenderTypeSolidShader { get; private set; }
    public static ShaderInstance? RenderTypeCutoutMippedShader { get; private set; }
    public static ShaderInstance? RenderTypeCutoutShader { get; private set; }
    public static ShaderInstance? RenderTypeTranslucentShader { get; private set; }
    public static ShaderInstance? RenderTypeTranslucentMovingBlockShader { get; private set; }
    public static ShaderInstance? RenderTypeTranslucentNoCrumblingShader { get; private set; }
    #endregion
    
    #region => Entity shader definitions
    public static ShaderInstance? RenderTypeArmorCutoutNoCullShader { get; private set; }
    public static ShaderInstance? RenderTypeEntitySolidShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityCutoutShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityCutoutNoCullShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityCutoutNoCullZOffsetShader { get; private set; }
    public static ShaderInstance? RenderTypeItemEntityTranslucentCullShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityTranslucentCullShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityTranslucentShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityTranslucentEmissiveShader { get; private set; }
    public static ShaderInstance? RenderTypeEntitySmoothCutoutShader { get; private set; }
    
    // This is not an entity shader, fyi
    public static ShaderInstance? RenderTypeBeaconBeamShader { get; private set; }
    
    public static ShaderInstance? RenderTypeEntityDecalShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityNoOutlineShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityShadowShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityAlphaShader { get; private set; }
    public static ShaderInstance? RenderTypeEyesShader { get; private set; }
    public static ShaderInstance? RenderTypeEnergySwirlShader { get; private set; }
    #endregion
    
    public static ShaderInstance? RenderTypeLeashShader { get; private set; }
    public static ShaderInstance? RenderTypeWaterMaskShader { get; private set; }
    public static ShaderInstance? RenderTypeOutlineShader { get; private set; }
    public static ShaderInstance? RenderTypeArmorGlintShader { get; private set; }
    public static ShaderInstance? RenderTypeArmorEntityGlintShader { get; private set; }
    public static ShaderInstance? RenderTypeGlintTranslucentShader { get; private set; }
    public static ShaderInstance? RenderTypeGlintShader { get; private set; }
    public static ShaderInstance? RenderTypeGlintDirectShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityGlintShader { get; private set; }
    public static ShaderInstance? RenderTypeEntityGlintDirectShader { get; private set; }
    public static ShaderInstance? RenderTypeTextShader { get; private set; }
    public static ShaderInstance? RenderTypeTextBackgroundShader { get; private set; }
    public static ShaderInstance? RenderTypeTextIntensityShader { get; private set; }
    public static ShaderInstance? RenderTypeTextSeeThroughShader { get; private set; }
    public static ShaderInstance? RenderTypeTextBackgroundSeeThroughShader { get; private set; }
    public static ShaderInstance? RenderTypeTextIntensitySeeThroughShader { get; private set; }
    public static ShaderInstance? RenderTypeLightningShader { get; private set; }
    public static ShaderInstance? RenderTypeTripwireShader { get; private set; }
    public static ShaderInstance? RenderTypeEndPortalShader { get; private set; }
    public static ShaderInstance? RenderTypeEndGatewayShader { get; private set; }
    public static ShaderInstance? RenderTypeLinesShader { get; private set; }
    public static ShaderInstance? RenderTypeCrumblingShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiOverlayShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiTextHighlightShader { get; private set; }
    public static ShaderInstance? RenderTypeGuiGhostRecipeOverlayShader { get; private set; }
    #endregion
    
    public LightTexture LightTexture { get; }
    public OverlayTexture OverlayTexture { get; } = new();

    public GameRenderer(Game game, ItemInHandRenderer itemInHandRenderer, IResourceManager resourceManager,
        RenderBuffers renderBuffers)
    {
        _game = game;
        _resourceManager = resourceManager;
        _itemInHandRenderer = itemInHandRenderer;
        LightTexture = new LightTexture(this, _game);
        _renderBuffers = renderBuffers;
    }

    private void LoadEffect(ResourceLocation location)
    {
        Util.LogFoobar();
    }

    private void ReloadShaders(IResourceProvider provider)
    {
        RenderSystem.AssertOnRenderThread();

        var list2 = new List<(Task<ShaderInstance>, Action<ShaderInstance>)>();
        var list3 = new ConcurrentBag<(ShaderInstance, Action<ShaderInstance>)>();
        var tcs = new TaskCompletionSource();

        try
        {
            #region => Basic shaders
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.Particle, DefaultVertexFormat.Particle),
                s => ParticleShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.Position, DefaultVertexFormat.Position),
                s => PositionShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.PositionColor, DefaultVertexFormat.PositionColor),
                s => PositionColorShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.PositionColorLightmap,
                    DefaultVertexFormat.PositionColorLightmap),
                s => PositionColorLightmapShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.PositionColorTex, DefaultVertexFormat.PositionColorTex),
                s => PositionColorTexShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.PositionColorTexLightmap,
                    DefaultVertexFormat.PositionColorTexLightmap),
                s => PositionColorTexLightmapShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.PositionTex, DefaultVertexFormat.PositionTex),
                s => PositionTexShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.PositionTexColor, DefaultVertexFormat.PositionTexColor),
                s => PositionTexColorShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.PositionTexColorNormal,
                    DefaultVertexFormat.PositionTexColorNormal),
                s => PositionTexColorNormalShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.PositionTexLightmapColor,
                    DefaultVertexFormat.PositionTexLightmapColor),
                s => PositionTexLightmapColorShader = s));
            #endregion

            #region => Block shaders
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeSolid, DefaultVertexFormat.Block),
                s => RenderTypeSolidShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeCutoutMipped, DefaultVertexFormat.Block), 
                s => RenderTypeCutoutMippedShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeCutout, DefaultVertexFormat.Block), 
                s => RenderTypeCutoutShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTranslucent, DefaultVertexFormat.Block), 
                s => RenderTypeTranslucentShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTranslucentMovingBlock, DefaultVertexFormat.Block), 
                s => RenderTypeTranslucentMovingBlockShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTranslucentNoCrumbling, DefaultVertexFormat.Block), 
                s => RenderTypeTranslucentNoCrumblingShader = s));
            #endregion

            #region => Entity shaders
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeArmorCutoutNoCull, DefaultVertexFormat.NewEntity), 
                s => RenderTypeArmorCutoutNoCullShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntitySolid, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntitySolidShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityCutout, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityCutoutShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityCutoutNoCull, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityCutoutNoCullShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityCutoutNoCullZOffset, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityCutoutNoCullZOffsetShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeItemEntityTranslucentCull, DefaultVertexFormat.NewEntity),
                s => RenderTypeItemEntityTranslucentCullShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityTranslucentCull, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityTranslucentCullShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityTranslucent, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityTranslucentShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityTranslucentEmissive, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityTranslucentEmissiveShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntitySmoothCutout, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntitySmoothCutoutShader = s));
            
            // This is not an entity shader, fyi
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeBeaconBeam, DefaultVertexFormat.Block),
                s => RenderTypeBeaconBeamShader = s));
            
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityDecal, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityDecalShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityNoOutline, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityNoOutlineShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityShadow, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityShadowShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityAlpha, DefaultVertexFormat.NewEntity),
                s => RenderTypeEntityAlphaShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEyes, DefaultVertexFormat.NewEntity),
                s => RenderTypeEyesShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEnergySwirl, DefaultVertexFormat.NewEntity),
                s => RenderTypeEnergySwirlShader = s));
            #endregion
            
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeLeash, DefaultVertexFormat.PositionColorLightmap),
                s => RenderTypeLeashShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeWaterMask, DefaultVertexFormat.Position),
                s => RenderTypeWaterMaskShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeOutline, DefaultVertexFormat.PositionColorTex),
                s => RenderTypeOutlineShader = s));
            
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeArmorGlint, DefaultVertexFormat.PositionTex),
                s => RenderTypeArmorGlintShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeArmorEntityGlint, DefaultVertexFormat.PositionTex),
                s => RenderTypeArmorEntityGlintShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGlintTranslucent, DefaultVertexFormat.PositionTex),
                s => RenderTypeGlintTranslucentShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGlint, DefaultVertexFormat.PositionTex),
                s => RenderTypeGlintShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGlintDirect, DefaultVertexFormat.PositionTex),
                s => RenderTypeGlintDirectShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityGlint, DefaultVertexFormat.PositionTex),
                s => RenderTypeEntityGlintShader = s));
            list2.Add((ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEntityGlintDirect, DefaultVertexFormat.PositionTex),
                s => RenderTypeEntityGlintDirectShader = s));
            
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeText, DefaultVertexFormat.PositionColorTexLightmap),
                s => RenderTypeTextShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTextBackground, DefaultVertexFormat.PositionColorLightmap),
                s => RenderTypeTextBackgroundShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTextIntensity, DefaultVertexFormat.PositionColorTexLightmap),
                s => RenderTypeTextIntensityShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTextSeeThrough, DefaultVertexFormat.PositionColorTexLightmap),
                s => RenderTypeTextSeeThroughShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTextBackgroundSeeThrough, DefaultVertexFormat.PositionColorLightmap),
                s => RenderTypeTextBackgroundSeeThroughShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTextIntensitySeeThrough, DefaultVertexFormat.PositionColorTexLightmap),
                s => RenderTypeTextIntensitySeeThroughShader = s));

            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeLightning, DefaultVertexFormat.PositionColor),
                s => RenderTypeLightningShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeTripwire, DefaultVertexFormat.Block),
                s => RenderTypeTripwireShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEndPortal, DefaultVertexFormat.Position),
                s => RenderTypeEndPortalShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeEndGateway, DefaultVertexFormat.Position),
                s => RenderTypeEndGatewayShader = s));
            
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeLines, DefaultVertexFormat.Position),
                s => RenderTypeLinesShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeCrumbling, DefaultVertexFormat.Position),
                s => RenderTypeCrumblingShader = s));
            
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGui, DefaultVertexFormat.PositionColor),
                s => RenderTypeGuiShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGuiOverlay, DefaultVertexFormat.PositionColor),
                s => RenderTypeGuiOverlayShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGuiTextHighlight, DefaultVertexFormat.PositionColor),
                s => RenderTypeGuiTextHighlightShader = s));
            list2.Add((
                ShaderInstance.CreateAsync(provider, ShaderNames.RenderTypeGuiGhostRecipeOverlay, DefaultVertexFormat.PositionColor),
                s => RenderTypeGuiGhostRecipeOverlayShader = s));

            var tasks = list2.Select(t => t.Item1).ToList();
            var times = new ConcurrentBag<double>();
            Logger.Info($"Compiling {tasks.Count} shaders...");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var i = 0; i < tasks.Count; i++)
            {
                var index = i;
                Task.Run(async () =>
                {
                    try
                    {
                        var shader = await tasks[index];
                        list3.Add((shader, list2[index].Item2));
                        times.Add(stopwatch.Elapsed.TotalMilliseconds);

                        if (list3.Count == tasks.Count)
                        {
                            tcs.SetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to load one of the shader");
                        Logger.Error(ex);
                        tcs.TrySetException(ex);
                    }
                });
            }
            
            tcs.Task.Wait();
            Logger.Info($"Compilation of {tasks.Count} shaders completed in {stopwatch.Elapsed.TotalMilliseconds} ms " +
                        $"(total: {times.Sum()} ms)");
        }
        catch (Exception ex)
        {
            foreach (var (shaderInstance, _) in list3)
            {
                shaderInstance.Dispose();
            }

            throw new Exception("Could not reload shaders", ex);
        }

        if (list2.Count != list3.Count)
        {
            throw new Exception($"Shader count mismatch with task count! Tasks = {list2.Count}, Shaders = {list3.Count}");
        }
        
        ShutdownShaders();
        foreach (var (shaderInstance, action) in list3)
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

    public void Render(float f, long l, bool bl)
    {
        if (_game.NoRender) return;
        var bl2 = _game.IsGameLoadFinished;
        var i = (int) (_game.MouseHandler.XPos * _game.Window.GuiScaledWidth / _game.Window.ScreenWidth);
        var j = (int) (_game.MouseHandler.YPos * _game.Window.GuiScaledHeight / _game.Window.ScreenHeight);

        var cl = GlStateManager.EnsureFramebufferSet();
        cl.SetViewport(0, new Viewport(0, 0, _game.Window.Width, _game.Window.Height, 0, 1));
        cl.ClearDepthStencil(1f);
        GlStateManager.SubmitCommands();

        var window = _game.Window;
        var matrix = Matrix4.CreateOrthographicOffCenter(0, (float) (window.Width / window.GuiScale),
            (float) (window.Height / window.GuiScale), 0, 1000, 21000);
        RenderSystem.SetProjectionMatrix(matrix, IVertexSorting.OrthoZ);
        
        var poseStack = RenderSystem.ModelViewStack;
        poseStack.PushPose();
        poseStack.SetIdentity();
        poseStack.Translate(0, 0, -11000);
        RenderSystem.ApplyModelViewMatrix();
        
        var graphics = new GuiGraphics(_game, _renderBuffers.BufferSource);
        if (_game.Overlay != null)
        {
            try
            {
                _game.Overlay.Render(graphics, i, j, _game.DeltaFrameTime);
            }
            catch (Exception ex)
            {
                var report = CrashReport.ForException(ex, "Rendering overlay");
                throw new ReportedException(report);
            }
        } else if (bl2 && _game.Screen != null)
        {
            try
            {
                _game.Screen.RenderWithTooltip(graphics, i, j, _game.DeltaFrameTime);
            }
            catch (Exception ex)
            {
                var report = CrashReport.ForException(ex, "Rendering screen");
                throw new ReportedException(report);
            }
        }
        
        graphics.Flush();
        poseStack.PopPose();
        RenderSystem.ApplyModelViewMatrix();
    }

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

    public record ResourceCache
        (IResourceProvider Original, Dictionary<ResourceLocation, Resource> Cache) : IResourceProvider
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