using MiaCrate.Data;

namespace MiaCrate.DataFixes.Schemas;

internal class V99 : Schema
{
    private static Dictionary<string, string> _itemToBlockEntity = new()
    {
        ["minecraft:furnace"] = "Furnace"
    };

    public V99(int versionKey, Schema? parent) : base(versionKey, parent)
    {
    }

    public override IDictionary<string, Func<ITypeTemplate>> RegisterEntities(Schema schema)
    {
        var map = new Dictionary<string, Func<ITypeTemplate>>();
        schema.RegisterSimple(map, "XPOrb");
        schema.RegisterSimple(map, "LeashKnot");
        schema.RegisterSimple(map, "Painting");
        Util.LogFoobar();
        return map;
    }

    public override IDictionary<string, Func<ITypeTemplate>> RegisterBlockEntities(Schema schema)
    {
        var map = new Dictionary<string, Func<ITypeTemplate>>();
        return map;
    }

    public override void RegisterTypes(Schema schema, IDictionary<string, Func<ITypeTemplate>> entityTypes, IDictionary<string, Func<ITypeTemplate>> blockEntityTypes)
    {
        schema.RegisterType(false, References.Level, Dsl.Remainder);
        schema.RegisterType(true, References.BlockEntity, Dsl.Remainder);
        Util.LogFoobar();
    }
}