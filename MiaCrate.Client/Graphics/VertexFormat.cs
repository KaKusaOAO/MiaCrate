using MiaCrate.Client.Systems;

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
}