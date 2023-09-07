using MiaCrate.Client.Systems;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public abstract class RenderStateShard
{
    protected readonly string _name;
    private readonly Action _setupState;
    private readonly Action _clearState;

    protected RenderStateShard(string name, Action setupState, Action clearState)
    {
        _name = name;
        _setupState = setupState;
        _clearState = clearState;
    }
    
    protected class ShaderStateShard : RenderStateShard
    {
        private readonly IOptional<Func<ShaderInstance>> _shader;

        public ShaderStateShard(Func<ShaderInstance> shader) : base("shader",
            () => RenderSystem.SetShader(shader),
            () => { })
        {
            _shader = Optional.Of(shader);
        }

        public ShaderStateShard() : base("shader",
            () => RenderSystem.SetShader(() => null),
            () => { })
        {
            _shader = Optional.Empty<Func<ShaderInstance>>();
        }
    }
}