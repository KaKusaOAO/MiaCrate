using OpenTK.Graphics.OpenGL;

namespace MiaCrate.Client.Platform;

public class BoolState
{
    private readonly EnableCap _state;
    private bool _enabled;

    public BoolState(EnableCap state)
    {
        _state = state;
    }

    public void Disable() => SetEnabled(false);

    public void Enable() => SetEnabled(true);

    public void SetEnabled(bool enabled)
    {
        if (enabled == _enabled) return;
        _enabled = enabled;
        
        if (enabled)
        {
            GL.Enable(_state);
        }
        else
        {
            GL.Disable(_state);
        }
    }
}