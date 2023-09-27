namespace MiaCrate.Client;

public class Options
{
    public OptionInstance<bool> Touchscreen { get; } = OptionInstance.CreateBool("options.touchscreen", false);

    public Options(Game game, string path)
    {
        
    }

    public void Save()
    {
        
    }
}