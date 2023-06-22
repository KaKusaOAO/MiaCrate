using MiaCrate.Data.Utils;

namespace MiaCrate.Data;

public class DataFixerBuilder
{
    private readonly SortedDictionary<int, Schema> _schemas = new();

    public int DataVersion { get; }
    
    public DataFixerBuilder(int dataVersion)
    {
        DataVersion = dataVersion;
    }

    public Schema AddVersion(int version, SchemaFactory factory) => AddVersion(version, 0, factory);
    
    public Schema AddVersion(int version, int subVersion, SchemaFactory factory)
    {
        var key = DataFixUtils.MakeKey(version, subVersion);
        var parent = !_schemas.Any() ? null : _schemas[DataFixerUpper.GetLowestSchemaSameVersion(_schemas, key - 1)];
        var schema = factory(DataFixUtils.MakeKey(version, subVersion), parent);
        AddSchema(schema);
        return schema;
    }

    public void AddSchema(Schema schema) => _schemas[schema.VersionKey] = schema;

    public void AddFixer(DataFix fix)
    {
        
    }
}