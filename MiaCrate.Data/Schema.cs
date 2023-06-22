using MiaCrate.Data.Utils;

namespace MiaCrate.Data;

public delegate Schema SchemaFactory(int versionKey, Schema? parent);
    
public class Schema
{
    public int VersionKey { get; }
    public Schema? Parent { get; }
    public string Name { get; }

    protected Dictionary<string, int> RecursiveTypes { get; } = new();
    private readonly Dictionary<string, Func<ITypeTemplate>> _typeTemplates = new();
    private readonly Dictionary<string, IDataType> _types = new();

    public ICollection<string> Types => _types.Keys;

    public Schema(int versionKey, Schema? parent)
    {
        VersionKey = versionKey;
        Parent = parent;

        var subVersion = DataFixUtils.GetSubVersion(versionKey);
        var version = DataFixUtils.GetVersion(versionKey);
        Name = $"V{version}" + (subVersion == 0 ? "" : $".{subVersion}");
    }

    public void InitializeTypes()
    {
        RegisterTypes(this);
        
        _types.Clear();
        foreach (var (k, v) in BuildTypes()) _types[k] = v;
    }

    protected IDictionary<string, IDataType> BuildTypes()
    {
        var types = new Dictionary<string, IDataType>();
        var templates = new List<ITypeTemplate>();

        foreach (var (k, v) in RecursiveTypes)
        {
            templates.Add(Dsl.Check(k, v, GetTemplate(k)));
        }

        var choice = templates.Aggregate(Dsl.Or);
        var family = new RecursiveTypeFamily(Name, choice);

        foreach (var name in _typeTemplates.Keys)
        {
            var recurseId = RecursiveTypes.GetValueOrDefault(name, -1);
            var type = recurseId != -1 ? family.Apply(recurseId) : GetTemplate(name).Apply(family).Apply(-1);
            types[name] = type;
        }

        return types;
    }

    protected ITypeTemplate GetTemplate(string name) => Dsl.Named(name, ResolveTemplate(name));

    public ITypeTemplate ResolveTemplate(string name) =>
        _typeTemplates.GetValueOrDefault(name, 
            () => throw new ArgumentException($"Unknown type: {name}"))();

    public void RegisterType(bool recursive, Dsl.ITypeReference type, Func<ITypeTemplate> template)
    {
        _typeTemplates[type.TypeName] = template;
        if (recursive) RecursiveTypes.TryAdd(type.TypeName, RecursiveTypes.Count);
    }

    public static SchemaFactory? CreateFactory<T>() where T : Schema
    {
        var ctor = typeof(T).GetConstructor(new[] { typeof(int), typeof(Schema) });
        if (ctor == null) return null;
        return (i, s) =>
        {
            var schema = (Schema)ctor.Invoke(new object?[] { i, s });
            schema.InitializeTypes();
            return schema;
        };
    }


    public virtual void RegisterTypes(Schema schema) => Parent!.RegisterTypes(schema);

    // public virtual void RegisterTypes(Schema schema, IDictionary<string, Func<ITypeTemplate>> entityTypes,
    //     IDictionary<string, Func<ITypeTemplate>> blockEntityTypes) =>
    //     Parent!.RegisterTypes(schema, entityTypes, blockEntityTypes);
    //
    // public virtual IDictionary<string, Func<ITypeTemplate>> RegisterEntities(Schema schema) =>
    //     Parent!.RegisterEntities(schema);
    //
    // public virtual IDictionary<string, Func<ITypeTemplate>> RegisterBlockEntities(Schema schema) =>
    //     Parent!.RegisterBlockEntities(schema);
}