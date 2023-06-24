namespace MiaCrate.Data;

public class DataFixerUpper : IDataFixer
{
    private readonly SortedDictionary<int, Schema> _schemas;
    private readonly List<DataFix> _globalList;
    public SortedSet<int> FixerVersions { get; }

    internal static IPointFreeRule OptimizationRule => throw new NotImplementedException();

    public DataFixerUpper(SortedDictionary<int, Schema> schemas, List<DataFix> globalList, SortedSet<int> fixerVersions)
    {
        _schemas = schemas;
        _globalList = globalList;
        FixerVersions = fixerVersions;
    }
    
    public static int GetLowestSchemaSameVersion(SortedDictionary<int, Schema> schemas, int versionKey)
    {
        if (versionKey < schemas.Keys.First()) return schemas.Keys.First();
        return schemas.Keys.TakeWhile(i => i < versionKey + 1).Last();
    }

    public IDynamic<T> Update<T>(Dsl.ITypeReference type, IDynamic<T> input, int version, int newVersion)
    {
        throw new NotImplementedException();
    }

    internal ITypeRewriteRule GetRule(int version, int dataVersion)
    {
        throw new NotImplementedException();
    }
}