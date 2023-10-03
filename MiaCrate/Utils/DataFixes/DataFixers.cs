using MiaCrate.Data;
using MiaCrate.DataFixes.Schemas;

namespace MiaCrate.DataFixes;

public static class DataFixers
{
    private static readonly SchemaFactory _same = (key, parent) => new Schema(key, parent); 
    
    public static IDataFixer DataFixer { get; } = CreateFixerUpper(SharedConstants.DataFixTypesToOptimize);

    private static IDataFixer CreateFixerUpper(HashSet<Dsl.ITypeReference> set)
    {
        var builder = new DataFixerBuilder(SharedConstants.CurrentVersion.DataVersion.Version);
        AddFixers(builder);

        if (!set.Any()) return builder.BuildUnoptimized();

        var executor = Util.BackgroundExecutor;
        return builder.BuildOptimized(set, executor);
    }

    private static void AddFixers(DataFixerBuilder builder)
    {
        builder.AddVersion(99, Schema.CreateFactory<V99>());

        var v100 = builder.AddVersion(100, Schema.CreateFactory<V100>());
        builder.AddFixer(new EntityEquipmentToArmorAndHandFix(v100, true));

        var v101 = builder.AddVersion(101, _same);
        builder.AddFixer(new BlockEntitySignTextStrictJsonFix(v101, false));

        Util.LogFoobar();
    }
}