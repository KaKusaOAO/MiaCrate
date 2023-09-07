namespace MiaCrate.Resources;

public interface IRepositorySource
{
    public void LoadPacks(Action<Pack> loader);
}