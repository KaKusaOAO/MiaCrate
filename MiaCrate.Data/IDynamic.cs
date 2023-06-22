namespace MiaCrate.Data;

public interface IDynamic : IDynamicLike
{
    
}

public interface IDynamic<T> : IDynamic, IDynamicLike<T>
{
}