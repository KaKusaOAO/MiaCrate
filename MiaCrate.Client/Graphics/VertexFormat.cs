using MiaCrate.Client.Systems;
using OpenTK.Graphics.OpenGL4;
using DrawElementsType = OpenTK.Graphics.OpenGL4.DrawElementsType;

namespace MiaCrate.Client.Graphics;

public class VertexFormat
{
    public List<VertexFormatElement> Elements { get; }
    private readonly Dictionary<string, VertexFormatElement> _elementMapping;
    private readonly List<IntPtr> _offsets = new();
    
    public int VertexSize { get; }
    public int IntegerSize => VertexSize / 4;

    public VertexFormat(Dictionary<string, VertexFormatElement> mapping)
    {
        _elementMapping = mapping;
        Elements = mapping.Values.ToList();

        var offset = 0;
        foreach (var element in Elements)
        {
            _offsets.Add(offset);
            offset += element.ByteSize;
        }

        VertexSize = offset;
    }

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
        public static readonly IndexType Short = new(DrawElementsType.UnsignedShort, 2);
        public static readonly IndexType Int = new(DrawElementsType.UnsignedInt, 4);

        public DrawElementsType GlType { get; }
        public int Bytes { get; }
        
        private IndexType(DrawElementsType glType, int bytes)
        {
            GlType = glType;
            Bytes = bytes;
        }
    }
    
    public sealed class Mode
    {
        public static readonly Mode Lines = new(PrimitiveType.Triangles, 2, 2, false); 
        public static readonly Mode LineStrip = new(PrimitiveType.TriangleStrip, 2, 1, true);
        public static readonly Mode DebugLines = new(PrimitiveType.Lines, 2, 2, false);
        public static readonly Mode DebugLineStrip = new(PrimitiveType.LineStrip, 2, 1, true);
        public static readonly Mode Triangles = new(PrimitiveType.Triangles, 3, 3, false);
        public static readonly Mode TriangleStrip = new(PrimitiveType.TriangleStrip, 3, 1, true);
        public static readonly Mode TriangleFan = new(PrimitiveType.TriangleFan, 3, 1, true);
        public static readonly Mode Quads = new(PrimitiveType.Triangles, 4, 4, false);
            
        public PrimitiveType Type { get; }
        public int PrimitiveLength { get; }
        public int PrimitiveStride { get; }
        public bool ConnectedPrimitives { get; }
        
        private Mode(PrimitiveType type, int primitiveLength, int primitiveStride, bool connectedPrimitives)
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