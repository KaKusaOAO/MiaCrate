namespace MiaCrate.Data;

public class RecursiveTypeFamily : ITypeFamily
{
    private static readonly Interner<ITypeTemplate> _interner = new();
    public string Name { get; }
    public ITypeTemplate Template { get; }
    public int Size { get; }
    private readonly int _hashCode;

    private readonly Dictionary<int, RecursivePoint.IRecursivePointType> _types = new();

    public RecursiveTypeFamily(string name, ITypeTemplate template)
    {
        Name = name;
        Template = _interner.Intern(template);
        Size = template.Size;
        _hashCode = HashCode.Combine(template);
    }
    
    public IType Apply(int index)
    {
        if (index < 0) throw new IndexOutOfRangeException();
        return _types.ComputeIfAbsent(index,
            i => new RecursivePoint.RecursivePointType(this, i, () => Template.Apply(this).Apply(i)));
    }

    public override int GetHashCode() => _hashCode;
}