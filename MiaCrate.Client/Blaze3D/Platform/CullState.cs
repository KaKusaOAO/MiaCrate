using Veldrid;

namespace MiaCrate.Client.Platform;

public class CullState
{
    public BoolState State { get; } = new();
    public FaceCullMode Mode { get; set; } = FaceCullMode.Back;
}