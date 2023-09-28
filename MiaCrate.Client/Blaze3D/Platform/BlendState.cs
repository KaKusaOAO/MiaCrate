using Veldrid;

namespace MiaCrate.Client.Platform;

public class BlendState
{
    public BoolState State { get; } = new();
    public BlendFactor SrcRgb { get; set; } = BlendFactor.One;
    public BlendFactor DstRgb { get; set; }
    public BlendFactor SrcAlpha { get; set; } = BlendFactor.One;
    public BlendFactor DstAlpha { get; set; }
}