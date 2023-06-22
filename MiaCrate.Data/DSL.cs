namespace MiaCrate.Data;

public static class Dsl
{
    public interface ITypeReference
    {
        string TypeName { get; }
        
        
    }

    public static ITypeTemplate Check(string name, int index, ITypeTemplate element)
    {
        
    }

    public static ITypeTemplate Named(string name, ITypeTemplate element)
    {
        throw new NotImplementedException();
    }

    public static ITypeTemplate Or(ITypeTemplate left, ITypeTemplate right)
    {
        throw new NotImplementedException();
    }
}