namespace MiaCrate.Core;

public interface IHolderLookup : IHolderGetter
{
    
}

public interface IHolderLookup<T> : IHolderLookup, IHolderGetter<T> where T : class
{
    
}