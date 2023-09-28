using Veldrid;

namespace MiaCrate.Client.Platform;

public class DepthState
{
    public BoolState State { get; } = new();
    public bool EnableMask { get; set; } = true;
    public ComparisonKind DepthFunction { get; set; } = ComparisonKind.Less;
}