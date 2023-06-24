namespace MiaCrate.Data;

public interface IDataFixer
{
    IDynamic<T> Update<T>(Dsl.ITypeReference type, IDynamic<T> input, int version, int newVersion);
}