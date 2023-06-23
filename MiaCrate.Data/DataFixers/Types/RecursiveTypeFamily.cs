namespace MiaCrate.Data;

public class RecursiveTypeFamily : ITypeFamily
{
    private static readonly Interner<ITypeTemplate> _interner = new();
    public string Name { get; }
    public ITypeTemplate Template { get; }
    public int Size { get; }
    private readonly int _hashCode;
    
    public RecursiveTypeFamily(string name, ITypeTemplate template)
    {
        Name = name;
        Template = _interner.Intern(template);
        Size = template.Size;
        _hashCode = HashCode.Combine(template);
    }
    
    public IType Apply(int index)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode() => _hashCode;
}