namespace MiaCrate.Client.Platform;

public class BoolState
{
    public bool IsEnabled { get; private set; }

    public BoolState()
    {
        
    }

    public void Disable() => SetEnabled(false);

    public void Enable() => SetEnabled(true);

    public void SetEnabled(bool enabled)
    {
        IsEnabled = enabled;
    }
}