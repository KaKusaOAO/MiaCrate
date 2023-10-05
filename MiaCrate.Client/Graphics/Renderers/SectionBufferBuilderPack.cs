namespace MiaCrate.Client.Graphics;

public class SectionBufferBuilderPack
{
    private readonly Dictionary<RenderType, BufferBuilder> _builders =
        RenderType.ChunkBufferLayers.ToDictionary(e => e, e => new BufferBuilder(e.BufferSize));

    public BufferBuilder GetBuilder(RenderType type) => _builders[type];

    public void ClearAll()
    {
        foreach (var builder in _builders.Values)
        {
            builder.Clear();
        }
    }
    
    public void DiscardAll()
    {
        foreach (var builder in _builders.Values)
        {
            builder.Discard();
        }
    }
}