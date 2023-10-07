using Mochi.Utils;

namespace MiaCrate.Client.Sodium.UI.Options;

public class GameOptionsStorage : IOptionStorage<Client.Options>
{
    private readonly Game _game;

    public GameOptionsStorage()
    {
        _game = Game.Instance;
    }

    public Client.Options Data => _game.Options;
    
    public void Save()
    {
        Data.Save();
        Logger.Info("Flushed changed to game config");
    }
}