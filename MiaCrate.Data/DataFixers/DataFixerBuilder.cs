using System.Diagnostics;
using MiaCrate.Common;
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
            Logger.Warn(FormattedText.Of("Ignored fix registered for version: %s as the DataVersion of the game is: %s")
                .AddWith(Component.Represent(version))
                .AddWith(Component.Represent(DataVersion))
            );
            return;
        }
        
        _globalList.Add(fix);
        _fixerVersions.Add(fix.VersionKey);
    }

    public IDataFixer BuildUnoptimized() => Build();

    public IDataFixer BuildOptimized(ISet<Dsl.ITypeReference> requiredTypes, IExecutor executor)
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

                var task = Tasks.RunAsync(() =>
                {
                    var dataType = schema.GetType(Dsl.CreateReference(() => typeName));
                    var rule = fixerUpper.GetRule(DataFixUtils.GetVersion(versionKey), DataVersion);
                    dataType.Rewrite(rule, DataFixerUpper.OptimizationRule);
                }, executor).ExceptionallyAsync(ex =>
                {
                    Logger.Error("Unable to build datafixers");
                    Logger.Error(ex);
                    Environment.Exit(1);
                });
                
                tasks.Add(task);
            }
        }

        Task.WhenAll(tasks.ToArray()).ThenRunAsync(() =>
        {
            Logger.Info(FormattedText.Of("%s datafixer optimizations took %s milliseconds")
                .AddWith(Component.Represent(tasks.Count))
                .AddWith(Component.Represent(stopwatch.Elapsed.TotalMilliseconds))
            );
        });
        
        return fixerUpper;
    }

    private DataFixerUpper Build() => new(
        new SortedDictionary<int, Schema>(_schemas), 
        new List<DataFix>(_globalList), 
        new SortedSet<int>(_fixerVersions));
}