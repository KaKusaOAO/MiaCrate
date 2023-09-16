using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public class ScissorState
{
    public BoolState State { get; } = new(EnableCap.ScissorTest);
}