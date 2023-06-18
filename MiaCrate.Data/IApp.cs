namespace MiaCrate.Data;

public interface IApp
{
}

public interface IAppLeft<T> : IApp where T : IK1
{
}

public interface IAppRight<T> : IApp
{
}

public interface IApp<TLeft, TRight> : IAppLeft<TLeft>, IAppRight<TRight> where TLeft : IK1
{
}