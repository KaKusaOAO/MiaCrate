using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public abstract class RenderType : RenderStateShard
{
    public static readonly RenderType Solid = Create("solid", DefaultVertexFormat.Block, VertexFormat.Mode.Quads,
        0x200000, true, false,
        CompositeState.Builder.CreateCompositeState(true));
    
    public VertexFormat Format { get; }
    public VertexFormat.Mode Mode { get; }
    public int BufferSize { get; }
    public bool AffectsCrumbling { get; }
    public bool SortOnUpload { get; }
    
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
    }

    public static CompositeRenderType Create(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
        CompositeState state) =>
        Create(name, format, mode, bufferSize, false, false, state);
    
    public static CompositeRenderType Create(string name, VertexFormat format, VertexFormat.Mode mode, int bufferSize,
        bool affectsCrumbling, bool sortOnUpload, CompositeState state) =>
        new(name, format, mode, bufferSize, affectsCrumbling, sortOnUpload, state);

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
        public List<RenderStateShard> States { get; }
        public OutlineProperty OutlineProperty { get; }
        
        private CompositeState(
            EmptyTextureStateShard textureState,
            ShaderStateShard shaderState,
            OutlineProperty outline
        )
        {
            TextureState = textureState;
            ShaderState = shaderState;
            OutlineProperty = outline;
            States = new List<RenderStateShard>
            {
                TextureState,
                ShaderState
            };
            throw new NotImplementedException();
        }

        public static CompositeStateBuilder Builder => new();

        public class CompositeStateBuilder
        {
            public EmptyTextureStateShard TextureState { get; set; }
            public ShaderStateShard ShaderState { get; set; }

            public CompositeStateBuilder SetShaderState(ShaderStateShard state)
            {
                ShaderState = state;
                return this;
            }

            public CompositeState CreateCompositeState(bool isOutline) =>
                CreateCompositeState(isOutline ? OutlineProperty.AffectsOutline : OutlineProperty.None);
            
            public CompositeState CreateCompositeState(OutlineProperty outline) =>
                new(
                    TextureState, ShaderState, 
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