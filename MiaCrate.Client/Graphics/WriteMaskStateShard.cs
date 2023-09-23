using MiaCrate.Client.Systems;

namespace MiaCrate.Client.Graphics;

public class WriteMaskStateShard : RenderStateShard
{
    private readonly bool _writeColor;
    private readonly bool _writeDepth;

    public WriteMaskStateShard(bool writeColor, bool writeDepth)
        : base("write_mask_state", () =>
        {
            if (!writeDepth)
            {
                RenderSystem.DepthMask(writeDepth);
            }

            if (!writeColor)
            {
                RenderSystem.ColorMask(writeColor, writeColor, writeColor, writeColor);
            }
        }, () =>
        {
            if (!writeDepth)
            {
                RenderSystem.DepthMask(true);
            }

            if (!writeColor)
            {
                RenderSystem.ColorMask(true, true, true, true);
            }
        })
    {
        _writeColor = writeColor;
        _writeDepth = writeDepth;
    }
}