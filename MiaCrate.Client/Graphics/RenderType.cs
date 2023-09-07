namespace MiaCrate.Client.Graphics;

public abstract class RenderType : RenderStateShard
{
    protected RenderType(string name, Action setupState, Action clearState) : base(name, setupState, clearState)
    {
    }
}