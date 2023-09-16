using MiaCrate.Client.Pipeline;
using MiaCrate.Resources;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class PostPass : IDisposable
{
    private readonly EffectInstance _effect;
    private readonly RenderTarget _inTarget;
    private readonly RenderTarget _outTarget;
    private readonly List<Func<int>> _auxSamplers = new();
    private readonly List<object> _auxAssets = new();
    private readonly List<string> _auxNames = new();
    private readonly List<int> _auxWidths = new();
    private readonly List<int> _auxHeights = new();
    private Matrix4 _shaderOrthoMatrix;

    public string Name => _effect.Name;

    public PostPass(IResourceManager manager, string name, RenderTarget inTarget, RenderTarget outTarget)
    {
        _effect = new EffectInstance(manager, name);
        _inTarget = inTarget;
        _outTarget = outTarget;
    }

    public void AddAuxAsset<T>(string name, T asset, Func<T, int> getSampler, int width, int height)
    {
        _auxNames.Add(name);
        _auxAssets.Add(asset!);
        _auxSamplers.Add(() => getSampler(asset));
        _auxWidths.Add(width);
        _auxHeights.Add(height);
    }

    public void SetOrthoMatrix(Matrix4 matrix)
    {
        _shaderOrthoMatrix = matrix;
    }

    public void Process(float f)
    {
        
        
        foreach (var asset in _auxAssets)
        {
            if (asset is RenderTarget target)
            {
                target.UnbindRead();
            }
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _effect.Dispose();
    }
}