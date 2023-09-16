using OpenTK.Graphics.OpenGL4;

namespace MiaCrate.Client.Platform;

public class ColorLogicState
{
    public BoolState State { get; } = new(EnableCap.ColorLogicOp);
    public LogicOp Op { get; set; } = LogicOp.Copy;
}