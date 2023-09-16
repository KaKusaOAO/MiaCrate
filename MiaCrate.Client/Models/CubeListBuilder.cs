using System.Collections.Immutable;

namespace MiaCrate.Client.Models;

public class CubeListBuilder
{
    private readonly List<CubeDefinition> _cubes = new();

    public static CubeListBuilder Create() => new();

    public IList<CubeDefinition> Cubes => _cubes.ToImmutableList();
}