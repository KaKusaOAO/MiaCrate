using MiaCrate.Extensions;

namespace MiaCrate.Client.Models;

public class PartDefinition
{
    private readonly IList<CubeDefinition> _cubes;
    private readonly PartPose _partPose;
    private readonly Dictionary<string, PartDefinition> _children = new();

    internal PartDefinition(IList<CubeDefinition> cubes, PartPose partPose)
    {
        _cubes = cubes;
        _partPose = partPose;
    }

    public PartDefinition AddOrReplaceChild(string name, CubeListBuilder cubeListBuilder, PartPose partPose)
    {
        var part = new PartDefinition(cubeListBuilder.Cubes, partPose);
        var old = _children.AddOrSet(name, part);

        if (old != null)
        {
            foreach (var (key, value) in old._children)
            {
                part._children[key] = value;
            }
        }

        return part;
    }
}