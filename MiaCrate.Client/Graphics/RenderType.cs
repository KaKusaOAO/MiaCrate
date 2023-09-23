using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public abstract class RenderType : RenderStateShard
{
    public static RenderType Solid { get; } = Create("solid", DefaultVertexFormat.Block, VertexFormat.Mode.Quads,
        0x200000, true, false,
        CompositeState.Builder
            .SetShaderState(RenderTypeSolidShader)
            .CreateCompositeState(true));

    public static RenderType CutoutMipped { get; } = Create("cutout_mipped", DefaultVertexFormat.Block,
        VertexFormat.Mode.Quads, 0x20000, true, false,
        CompositeState.Builder
            .SetShaderState(RenderTypeCutoutMippedShader)
            .CreateCompositeState(false));

    private static readonly Func<ResourceLocation, bool, RenderType> _entityCutoutNoCull = Util.Memoize(
        (ResourceLocation location, bool outline) =>
        {
            var state = CompositeState.Builder
                .SetShaderState(RenderTypeEntityCutoutNoCullShader)
                .SetTextureState(new TextureStateShard(location, false, false))
                .SetTransparencyState(NoTransparency)
                .SetCullState(NoCull)
                .CreateCompositeState(outline);
                
            return Create("entity_cutout_no_cull", DefaultVertexFormat.NewEntity, VertexFormat.Mode.Quads, 0x100,
                true, false, state);
        });

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
        Solid, CutoutMipped, // Cutout, Translucent, Tripwire
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

    public static RenderType EntityCutoutNoCull(ResourceLocation location, bool outline = true) => 
        _entityCutoutNoCull(location, outline);

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
        public WriteMaskStateShard WriteMaskState { get; }
        public List<RenderStateShard> States { get; }
        public OutlineProperty OutlineProperty { get; }
        
        private CompositeState(
            EmptyTextureStateShard textureState,
            ShaderStateShard shaderState,
            TransparencyStateShard transparencyState,
            DepthTestStateShard depthTestState,
            CullStateShard cullState,
            WriteMaskStateShard writeMaskState,
            OutlineProperty outline
        )
        {
            TextureState = textureState;
            ShaderState = shaderState;
            TransparencyState = transparencyState;
            DepthTestState = depthTestState;
            CullState = cullState;
            WriteMaskState = writeMaskState;
            
            Util.LogFoobar();
            
            OutlineProperty = outline;
            States = new List<RenderStateShard>
            {
                TextureState,
                ShaderState,
                TransparencyState,
                DepthTestState
            };
        }

        public static CompositeStateBuilder Builder => new();

        public class CompositeStateBuilder
        {
            public EmptyTextureStateShard TextureState { get; set; } = NoTexture;
            public ShaderStateShard ShaderState { get; set; } = NoShader;
            public TransparencyStateShard TransparencyState { get; set; } = NoTransparency;
            public DepthTestStateShard DepthTestState { get; set; } = LequalDepthTest;
            public WriteMaskStateShard WriteMaskState { get; set; } = ColorDepthWrite;
            public CullStateShard CullState { get; set; } = Cull;

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

            public CompositeStateBuilder SetWriteMaskState(WriteMaskStateShard state)
            {
                WriteMaskState = state;
                return this;
            }

            public CompositeState CreateCompositeState(bool isOutline) =>
                CreateCompositeState(isOutline ? OutlineProperty.AffectsOutline : OutlineProperty.None);
            
            public CompositeState CreateCompositeState(OutlineProperty outline) =>
                new(
                    TextureState, ShaderState, TransparencyState, DepthTestState, CullState, WriteMaskState,
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