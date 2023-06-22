namespace MiaCrate.Data;

public interface IDataType : IApp
{
    public class Mu : IK1 {}
}

public interface IDataType<T> : IDataType, IApp<IDataType.Mu, T>
{
    
}

public static class DataType
{
    public static IDataType<T> Unbox<T>(IApp<IDataType.Mu, T> box) => (IDataType<T>)box;
}