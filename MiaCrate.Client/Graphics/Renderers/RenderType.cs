using MiaCrate.Client.Shaders;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public abstract class RenderType : RenderStateShard
{
    public static RenderType Solid { get; } = Create("solid", DefaultVertexFormat.Block, VertexFormat.Mode.Quads,
        0x200000, true, false,
        CompositeState.Builder
            .SetLightmapState(Lightmap)
            .SetShaderState(RenderTypeSolidShader)
            .SetTextureState(BlockSheetMipped)
            .CreateCompositeState(true));

    public static RenderType CutoutMipped { get; } = Create("cutout_mipped", DefaultVertexFormat.Block,
        VertexFormat.Mode.Quads, 0x20000, true, false,
        CompositeState.Builder
            .SetLightmapState(Lightmap)
            .SetShaderState(RenderTypeCutoutMippedShader)
            .SetTextureState(BlockSheetMipped)
            .CreateCompositeState(true));
        
    public static RenderType Cutout { get; } = Create("cutout", DefaultVertexFormat.Block,
        VertexFormat.Mode.Quads, 0x20000, true, false,
        CompositeState.Builder
            .SetLightmapState(Lightmap)
            .SetShaderState(RenderTypeCutoutShader)
            .SetTextureState(BlockSheet)
            .CreateCompositeState(true));
    
    public static RenderType Translucent { get; } = Create("translucent", DefaultVertexFormat.Block,
        VertexFormat.Mode.Quads, 0x200000, true, true,
        CreateTranslucentState(RenderTypeTranslucentShader));

    private static readonly Func<ResourceLocation, bool, RenderType> _entityCutoutNoCull = Util.Memoize(
        (ResourceLocation location, bool outline) =>
        {
            var state = CompositeState.Builder
                .SetShaderState(RenderTypeEntityCutoutNoCullShader)
                .SetTextureState(new TextureStateShard(location, false, false))
                .SetTransparencyState(NoTransparency)
                .SetCullState(NoCull)
                .SetLightmapState(Lightmap)
                .SetOverlayState(Overlay)
                .CreateCompositeState(outline);
                
            return Create("entity_cutout_no_cull", DefaultVertexFormat.NewEntity, VertexFormat.Mode.Quads, 0x100,
                true, false, state);
        });

    private static readonly Func<ResourceLocation, RenderType> _textIntensity = Util.Memoize((ResourceLocation l) =>
        Create("text_intensity", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextIntensityShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .CreateCompositeState(false)));
    
    private static readonly Func<ResourceLocation, RenderType> _textIntensityPolygonOffset = Util.Memoize((ResourceLocation l) =>
        Create("text_intensity_polygon_offset", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextIntensityShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .SetLayeringState(PolygonOffsetLayering)
                .CreateCompositeState(false)));
    
    private static readonly Func<ResourceLocation, RenderType> _textIntensitySeeThrough = Util.Memoize((ResourceLocation l) =>
        Create("text_intensity_see_through", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextIntensitySeeThroughShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .SetDepthTestState(NoDepthTest)
                .SetWriteMaskState(ColorWrite)
                .CreateCompositeState(false)));
    
    private static readonly Func<ResourceLocation, RenderType> _text = Util.Memoize((ResourceLocation l) =>
        Create("text", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .CreateCompositeState(false)));
    
    private static readonly Func<ResourceLocation, RenderType> _textPolygonOffset = Util.Memoize((ResourceLocation l) =>
        Create("text_polygon_offset", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .SetLayeringState(PolygonOffsetLayering)
                .CreateCompositeState(false)));
    
    private static readonly Func<ResourceLocation, RenderType> _textSeeThrough = Util.Memoize((ResourceLocation l) =>
        Create("text_see_through", DefaultVertexFormat.PositionColorTexLightmap,
            VertexFormat.Mode.Quads, 0x100, false, true,
            CompositeState.Builder
                .SetShaderState(RenderTypeTextSeeThroughShader)
                .SetTextureState(new TextureStateShard(l, false, false))
                .SetTransparencyState(TranslucentTransparency)
                .SetLightmapState(Lightmap)
                .SetDepthTestState(NoDepthTest)
                .SetWriteMaskState(ColorWrite)
                .CreateCompositeState(false)));

    public static RenderType Gui { get; } = Create("gui", DefaultVertexFormat.PositionColor,
        VertexFormat.Mode.Quads, 0x100,
        CompositeState.Builder
            .SetShaderState(RenderTypeGuiShader)
            .SetTransparencyState(TranslucentTransparency)
            .SetDepthTestState(LequalDepthTest)
            .CreateCompositeState(false));

    public static RenderType GuiOverlay { get; } = Create("gui_overlay", DefaultVertexFormat.PositionColor,
        VertexFormat.Mode.Quads, 0x100,
        CompositeState.Builder
            .SetShaderState(RenderTypeGuiOverlayShader)
            .SetTransparencyState(TranslucentTransparency)
            .SetDepthTestState(NoDepthTest)
            .SetWriteMaskState(ColorWrite)
            .CreateCompositeState(false));

    public static List<RenderType> ChunkBufferLayers { get; } = new()
    {
        Solid, CutoutMipped, Cutout, Translucent, // Tripwire
    };

    private readonly IOptional<RenderType> _asOptional;
    
    public VertexFormat Format { get; }
    public VertexFormat.Mode Mode { get; }
    public int BufferSize { get; }
    public bool AffectsCrumbling { get; }
    public bool SortOnUpload { get; }
    public bool CanConsolidateConsecutiveGeometry => !Mode.ConnectedPrimitives;
    
    public virtual IOptional<RenderType> Outline => Optional.Empty<RenderType>();
    public virtual bool IsOutline => false;

    protected RenderType(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
        bool affectsCrumbling, bool sortOnUpload, Action setupState, Action clearState) 
        : base(name, setupState, clearState)
    {
        Format = format;
        Mode = mode;
        BufferSize = bufferSize;
        AffectsCrumbling = affectsCrumbling;
        SortOnUpload = sortOnUpload;
        _asOptional = Optional.Of(this);
    }

    public IOptional<RenderType> AsOptional() => _asOptional;

    public static CompositeRenderType Create(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
        CompositeState state) =>
        Create(name, format, mode, bufferSize, false, false, state);
    
    public static CompositeRenderType Create(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
        bool affectsCrumbling, bool sortOnUpload, CompositeState state) =>
        new(name, format, mode, bufferSize, affectsCrumbling, sortOnUpload, state);

    public static CompositeState CreateTranslucentState(ShaderStateShard state)
    {
        return CompositeState.Builder
            .SetLightmapState(Lightmap)
            .SetShaderState(state)
            .SetTextureState(BlockSheetMipped)
            .SetTransparencyState(TranslucentTransparency)
            .SetOutputState(TranslucentTarget)
            .CreateCompositeState(true);
    }

    public static RenderType EntityCutoutNoCull(ResourceLocation location, bool outline = true) => 
        _entityCutoutNoCull(location, outline);
    
    public static RenderType TextIntensity(ResourceLocation location) => 
        _textIntensity(location);
    
    public static RenderType TextIntensitySeeThrough(ResourceLocation location) => 
        _textIntensitySeeThrough(location);
    
    public static RenderType TextIntensityPolygonOffset(ResourceLocation location) => 
        _textIntensityPolygonOffset(location);
    
    public static RenderType Text(ResourceLocation location) => 
        _text(location);
    
    public static RenderType TextSeeThrough(ResourceLocation location) => 
        _textSeeThrough(location);
    
    public static RenderType TextPolygonOffset(ResourceLocation location) => 
        _textPolygonOffset(location);

    public void End(BufferBuilder builder, IVertexSorting vertexSorting)
    {
        if (!builder.IsBuilding) return;
        if (SortOnUpload) builder.SetQuadSorting(vertexSorting);

        var buffer = builder.End();
        SetupRenderState();
        BufferUploader.DrawWithShader(buffer);
        ClearRenderState();
    }

    public class CompositeRenderType : RenderType
    {
        public CompositeState State { get; }
        public override IOptional<RenderType> Outline { get; }
        public override bool IsOutline { get; }

        public CompositeRenderType(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
            bool affectsCrumbling, bool sortOnUpload, CompositeState state)
            : base(name, format, mode, bufferSize, affectsCrumbling, sortOnUpload,
                () => state.States.ForEach(s => s.SetupRenderState()),
                () => state.States.ForEach(s => s.ClearRenderState())
            )
        {
            State = state;
            Outline = state.OutlineProperty == OutlineProperty.AffectsOutline
                ? state.TextureState.CutoutTexture.Select<RenderType>(_ => throw new NotImplementedException())
                : Optional.Empty<RenderType>();
            IsOutline = state.OutlineProperty == OutlineProperty.IsOutline;
        }
        
        public override string ToString() => $"RenderType[{base.ToString()}:{State}]";
    }

    public class CompositeState
    {
        public EmptyTextureStateShard TextureState { get; }
        public ShaderStateShard ShaderState { get; }
        public TransparencyStateShard TransparencyState { get; }
        public DepthTestStateShard DepthTestState { get; }
        public CullStateShard CullState { get; }
        public LightmapStateShard LightmapState { get; }
        public LayeringStateShard LayeringState { get; }
        public WriteMaskStateShard WriteMaskState { get; }
        public List<RenderStateShard> States { get; }
        public OutlineProperty OutlineProperty { get; }
        
        private CompositeState(
            EmptyTextureStateShard textureState,
            ShaderStateShard shaderState,
            TransparencyStateShard transparencyState,
            DepthTestStateShard depthTestState,
            CullStateShard cullState,
            LightmapStateShard lightmapState,
            LayeringStateShard layeringState,
            WriteMaskStateShard writeMaskState,
            OutlineProperty outline
        )
        {
            TextureState = textureState;
            ShaderState = shaderState;
            TransparencyState = transparencyState;
            DepthTestState = depthTestState;
            CullState = cullState;
            LightmapState = lightmapState;
            LayeringState = layeringState;
            WriteMaskState = writeMaskState;
            
            Util.LogFoobar();
            
            OutlineProperty = outline;
            States = new List<RenderStateShard>
            {
                TextureState,
                ShaderState,
                TransparencyState,
                DepthTestState,
                CullState,
                LightmapState,
                LayeringState,
                WriteMaskState
            };
        }

        public static CompositeStateBuilder Builder => new();

        public class CompositeStateBuilder
        {
            public EmptyTextureStateShard TextureState { get; set; } = NoTexture;
            public ShaderStateShard ShaderState { get; set; } = NoShader;
            public TransparencyStateShard TransparencyState { get; set; } = NoTransparency;
            public DepthTestStateShard DepthTestState { get; set; } = LequalDepthTest;
            public CullStateShard CullState { get; set; } = Cull;
            public LightmapStateShard LightmapState { get; set; } = NoLightmap;
            public OverlayStateShard OverlayState { get; set; } = NoOverlay;
            public LayeringStateShard LayeringState { get; set; } = NoLayering;
            public OutputStateShard OutputState { get; set; } = MainTarget;
            public WriteMaskStateShard WriteMaskState { get; set; } = ColorDepthWrite;

            public CompositeStateBuilder SetTextureState(EmptyTextureStateShard state)
            {
                TextureState = state;
                return this;
            }

            public CompositeStateBuilder SetShaderState(ShaderStateShard state)
            {
                ShaderState = state;
                return this;
            }

            public CompositeStateBuilder SetTransparencyState(TransparencyStateShard state)
            {
                TransparencyState = state;
                return this;
            }

            public CompositeStateBuilder SetDepthTestState(DepthTestStateShard state)
            {
                DepthTestState = state;
                return this;
            }

            public CompositeStateBuilder SetCullState(CullStateShard state)
            {
                CullState = state;
                return this;
            }

            public CompositeStateBuilder SetLightmapState(LightmapStateShard state)
            {
                LightmapState = state;
                return this;
            }
            
            public CompositeStateBuilder SetOverlayState(OverlayStateShard state)
            {
                OverlayState = state;
                return this;
            }

            public CompositeStateBuilder SetLayeringState(LayeringStateShard state)
            {
                LayeringState = state;
                return this;
            }

            public CompositeStateBuilder SetOutputState(OutputStateShard state)
            {
                OutputState = state;
                return this;
            }
            
            public CompositeStateBuilder SetWriteMaskState(WriteMaskStateShard state)
            {
                WriteMaskState = state;
                return this;
            }

            public CompositeState CreateCompositeState(bool isOutline) =>
                CreateCompositeState(isOutline ? OutlineProperty.AffectsOutline : OutlineProperty.None);
            
            public CompositeState CreateCompositeState(OutlineProperty outline) =>
                new(
                    TextureState, ShaderState, TransparencyState, DepthTestState, CullState, LightmapState, 
                    LayeringState, WriteMaskState,
                    outline
                );
        }
    }

    public sealed class OutlineProperty
    {
        public static readonly OutlineProperty None = new("none");
        public static readonly OutlineProperty IsOutline = new("is_outline");
        public static readonly OutlineProperty AffectsOutline = new("affects_outline");
        
        private readonly string _name;
        
        private OutlineProperty(string name)
        {
            _name = name;
        }

        public override string ToString() => _name;
    }
}