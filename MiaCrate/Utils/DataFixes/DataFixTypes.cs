using MiaCrate.Data;

namespace MiaCrate.DataFixes;

public sealed class DataFixTypes
{
    public static DataFixTypes Level { get; } = new(References.Level);

    public static HashSet<Dsl.ITypeReference> TypesForLevelList { get; } = new() {Level._type};

    private readonly Dsl.ITypeReference _type;

    private DataFixTypes(Dsl.ITypeReference type)
    {
        _type = type;
    }
}