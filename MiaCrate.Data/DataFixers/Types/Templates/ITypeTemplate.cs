namespace MiaCrate.Data;

public interface ITypeTemplate
{
    int Size { get; }
    ITypeFamily Apply(ITypeFamily family);
}