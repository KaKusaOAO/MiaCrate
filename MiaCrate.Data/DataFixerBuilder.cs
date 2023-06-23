using System.Diagnostics;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Data;

public class DataFixerBuilder
{
    private readonly SortedDictionary<int, Schema> _schemas = new();
    private readonly List<DataFix> _globalList = new();
    private readonly SortedSet<int> _fixerVersions = new();

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
        var version = DataFixUtils.GetVersion(fix.VersionKey);
        if (version > DataVersion)
        {
            Logger.Warn(TranslateText.Of("Ignored fix registered for version: %s as the DataVersion of the game is: %s")
                .AddWith(Text.Represent(version))
                .AddWith(Text.Represent(DataVersion))
            );
            return;
        }
        
        _globalList.Add(fix);
        _fixerVersions.Add(fix.VersionKey);
    }

    public IDataFixer BuildUnoptimized() => Build();

    public IDataFixer BuildOptimized(ISet<Dsl.ITypeReference> requiredTypes)
    {
        var fixerUpper = Build();
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var tasks = new List<Task>();
        var requiredTypeNames = requiredTypes.Select(t => t.TypeName).ToHashSet();

        foreach (var versionKey in fixerUpper.FixerVersions)
        {
            var schema = _schemas[versionKey];
            foreach (var typeName in schema.Types)
            {
                if (!requiredTypeNames.Contains(typeName)) continue;
                var task = Task.Run(() =>
                {
                    var dataType = schema.GetType(Dsl.CreateReference(() => typeName));
                    var rule = fixerUpper.GetRule(DataFixUtils.GetVersion(versionKey), DataVersion);
                    dataType.Rewrite(rule, DataFixerUpper.OptimizationRule);
                }).ContinueWith(t =>
                {
                    if (!t.IsFaulted) return;
                    var e = t.Exception!;
                    Logger.Error("Unable to build datafixers");
                    Logger.Error(e);
                });
                tasks.Add(task);
            }
        }

        Task.WhenAll(tasks.ToArray()).ContinueWith(_ =>
        {
            Logger.Info(TranslateText.Of("%s datafixer optimizations took %s milliseconds")
                .AddWith(Text.Represent(tasks.Count))
                .AddWith(Text.Represent(stopwatch.Elapsed.TotalMilliseconds))
            );
        });
        
        return fixerUpper;
    }

    private DataFixerUpper Build() => new(
        new SortedDictionary<int, Schema>(_schemas), 
        new List<DataFix>(_globalList), 
        new SortedSet<int>(_fixerVersions));
}