namespace MiaCrate.Client.Graphics;

public class RenderBuffers
{
    private readonly SectionBufferBuilderPack _fixedBufferPack = new();
    private readonly Dictionary<RenderType, BufferBuilder> _fixedBuffers = Util.Make(
        new SortedDictionary<RenderType, BufferBuilder>(),
        d =>
        {
            // d[Sheets.SolidBlockSheet] = 
        }).ToDictionary(e => e.Key, e => e.Value);

    public IMultiBufferSource.BufferSource BufferSource { get; }

    public RenderBuffers()
    {
        BufferSource = IMultiBufferSource.ImmediateWithBuffers(_fixedBuffers, new BufferBuilder(0x100));
    }
}