using MiaCrate.Client.Systems;
using Veldrid;

namespace MiaCrate.Client;

public class VertexFormat
{
    public List<VertexFormatElement> Elements { get; }
    private readonly Dictionary<string, VertexFormatElement> _elementMapping;
    private readonly Func<VertexLayoutDescription> _func;
    private readonly List<IntPtr> _offsets = new();
    private VertexBuffer? _immediateDrawVertexBuffer;
    
    public int VertexSize { get; }
    public int IntegerSize => VertexSize / 4;

    public VertexFormat(Dictionary<string, VertexFormatElement> mapping, Func<VertexLayoutDescription> func)
    {
        _elementMapping = mapping;
        _func = func;
        Elements = mapping.Values.ToList();

        var offset = 0;
        foreach (var element in Elements)
        {
            _offsets.Add(offset);
            offset += element.ByteSize;
        }

        VertexSize = offset;
    }

    public VertexBuffer ImmediateDrawVertexBuffer =>
        _immediateDrawVertexBuffer ??= new VertexBuffer();

    public VertexLayoutDescription CreateVertexLayoutDescription() => _func();

    public void SetupBufferState()
    {
        if (!RenderSystem.IsOnRenderThread)
            RenderSystem.RecordRenderCall(InternalSetupBufferState);
        else
            InternalSetupBufferState();
    }

    private void InternalSetupBufferState()
    {
        var stride = VertexSize;

        for (var index = 0; index < Elements.Count; index++)
        {
            var element = Elements[index];
            element.SetupBufferState(index, _offsets[index], stride);
        }
    }

    public void ClearBufferState()
    {
        if (!RenderSystem.IsOnRenderThread)
            RenderSystem.RecordRenderCall(InternalClearBufferState);
        else
            InternalClearBufferState();
    }

    private void InternalClearBufferState()
    {
        for (var index = 0; index < Elements.Count; index++)
        {
            var element = Elements[index];
            element.ClearBufferState(index);
        }
    }

    public List<string> ElementAttributeNames => _elementMapping.Keys.ToList();

    public sealed class IndexType
    {
        public static readonly IndexType Short = new(IndexFormat.UInt16, 2);
        public static readonly IndexType Int = new(IndexFormat.UInt32, 4);

        public IndexFormat Format { get; }
        public int Bytes { get; }
        
        private IndexType(IndexFormat format, int bytes)
        {
            Format = format;
            Bytes = bytes;
        }

        public static IndexType Least(int i) => (i & -0x10000) != 0 ? Int : Short;
    }
    
    public sealed class Mode
    {
        public static readonly Mode Lines = new(PrimitiveTopology.TriangleList, 2, 2, false); 
        public static readonly Mode LineStrip = new(PrimitiveTopology.TriangleStrip, 2, 1, true);
        public static readonly Mode DebugLines = new(PrimitiveTopology.LineList, 2, 2, false);
        public static readonly Mode DebugLineStrip = new(PrimitiveTopology.LineStrip, 2, 1, true);
        public static readonly Mode Triangles = new(PrimitiveTopology.TriangleList, 3, 3, false);
        public static readonly Mode TriangleStrip = new(PrimitiveTopology.TriangleStrip, 3, 1, true);
        public static readonly Mode TriangleFan = new(PrimitiveTopology.TriangleStrip, 3, 1, true);
        public static readonly Mode Quads = new(PrimitiveTopology.TriangleList, 4, 4, false);
            
        public PrimitiveTopology Type { get; }
        public int PrimitiveLength { get; }
        public int PrimitiveStride { get; }
        public bool ConnectedPrimitives { get; }
        
        private Mode(PrimitiveTopology type, int primitiveLength, int primitiveStride, bool connectedPrimitives)
        {
            Type = type;
            PrimitiveLength = primitiveLength;
            PrimitiveStride = primitiveStride;
            ConnectedPrimitives = connectedPrimitives;
        }

        public int GetIndexCount(int i)
        {
            if (this == LineStrip || this == DebugLines || this == DebugLineStrip || 
                this == Triangles || this == TriangleStrip || this == TriangleFan)
            {
                return i;
            }

            if (this == Lines || this == Quads)
            {
                return i / 4 * 6;
            }

            return 0;
        }
    }
}