using MiaCrate.Data;

namespace MiaCrate.DataFixes;

public abstract class NamedEntityFix : DataFix
{
    private readonly string _name;
    private readonly Dsl.ITypeReference _type;
    private readonly string _entityName;

    protected NamedEntityFix(Schema outputSchema, bool changesType, string name, Dsl.ITypeReference type, string entityName) 
        : base(outputSchema, changesType)
    {
        _name = name;
        _type = type;
        _entityName = entityName;
    }
}