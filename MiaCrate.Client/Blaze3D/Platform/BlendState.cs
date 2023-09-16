using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public class BlendState
{
    public BoolState State { get; } = new(EnableCap.Blend);
    public BlendingFactorSrc SrcRgb { get; set; } = BlendingFactorSrc.One;
    public BlendingFactorDest DstRgb { get; set; }
    public BlendingFactorSrc SrcAlpha { get; set; } = BlendingFactorSrc.One;
    public BlendingFactorDest DstAlpha { get; set; }
}