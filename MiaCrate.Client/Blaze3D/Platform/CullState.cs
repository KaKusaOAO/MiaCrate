using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public class CullState
{
    public BoolState State { get; } = new(EnableCap.CullFace);
    public CullFaceMode Mode { get; set; } = CullFaceMode.Back;
}