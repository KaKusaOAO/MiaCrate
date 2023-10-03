using MiaCrate.Data;

namespace MiaCrate.DataFixes.Schemas;

public class V100 : Schema
{
    public V100(int versionKey, Schema? parent) : base(versionKey, parent)
    {
    }

    private static ITypeTemplate Equipment(Schema schema)
    {
        Util.LogFoobar();
        return Dsl.Remainder();
    }
    
    private static void RegisterMob(Schema schema, IDictionary<string, Func<ITypeTemplate>> map, string name) => 
        schema.Register(map, name, () => Equipment(schema));

    public override IDictionary<string, Func<ITypeTemplate>> RegisterEntities(Schema schema)
    {
        var map = base.RegisterEntities(schema);
        RegisterMob(schema, map, "ArmorStand");
        RegisterMob(schema, map, "Creeper");
        RegisterMob(schema, map, "Skeleton");
        RegisterMob(schema, map, "Spider");
        RegisterMob(schema, map, "Giant");
        RegisterMob(schema, map, "Zombie");
        RegisterMob(schema, map, "Slime");
        RegisterMob(schema, map, "Ghast");
        RegisterMob(schema, map, "PigZombie");
        Util.LogFoobar();
        return map;
    }

    public override IDictionary<string, Func<ITypeTemplate>> RegisterBlockEntities(Schema schema)
    {
        var map = new Dictionary<string, Func<ITypeTemplate>>();
        Util.LogFoobar();
        return map;
    }
}