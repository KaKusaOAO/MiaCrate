namespace MiaCrate.Client.Sodium.UI.Options;

public class SodiumOptionsStorage : IOptionStorage<SodiumOptions>
{
    public SodiumOptions Data { get; }
    
    public SodiumOptionsStorage()
    {
        Data = new SodiumOptions();
    }

    public void Save()
    {
        throw new NotImplementedException();
    }
}