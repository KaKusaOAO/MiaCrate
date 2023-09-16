namespace MiaCrate.Client.Models;

public class MeshDefinition
{
    public PartDefinition Root { get; } = new(new List<CubeDefinition>(), PartPose.Zero);
}