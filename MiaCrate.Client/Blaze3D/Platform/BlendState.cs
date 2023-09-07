using OpenTK.Graphics.OpenGL;

namespace MiaCrate.Client.Platform;

public class BlendState
{
    public BoolState State { get; } = new(EnableCap.Blend);
    public int SrcRgb { get; set; } = 1;
    public int DstRgb { get; set; }
    public int SrcAlpha { get; set; } = 1;
    public int DstAlpha { get; set; }
}