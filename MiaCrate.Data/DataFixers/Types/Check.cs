namespace MiaCrate.Data;

public sealed class Check : ITypeTemplate
{
    public string Name { get; }
    public int Index { get; }
    public ITypeTemplate Element { get; }
    public int Size => Math.Max(Index + 1, Element.Size);

    public Check(string name, int index, ITypeTemplate element)
    {
        Name = name;
        Index = index;
        Element = element;
    }
    
    public ITypeFamily Apply(ITypeFamily family)
    {
        throw new NotImplementedException();
    }
}