namespace MiaCrate.Client.Audio;

public class Library
{
    private string? _defaultDeviceName;

    public Listener Listener { get; } = new();

    public Library()
    {
        _defaultDeviceName = DefaultDeviceName;
    }

    public void Init(string? str, bool bl)
    {
        Util.LogFoobar();
    }
    
    public static string? DefaultDeviceName
    {
        get
        {
            Util.LogFoobar();
            return null;
        }
    }
}