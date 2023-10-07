namespace MiaCrate.Client.Sodium.UI.Options;

public interface IOptionStorage
{
    public void Save();
}

public interface IOptionStorage<out T> : IOptionStorage
{
    public T Data { get; }
}