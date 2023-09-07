namespace MiaCrate.Core;

public interface IHolderOwner
{
    public bool CanSerializeIn(IHolderOwner owner) => owner == this;
}

public interface IHolderOwner<T> : IHolderOwner
{
    public bool CanSerializeIn(IHolderOwner<T> owner) => owner == this;
}