using MiaCrate.Client.Pipeline;
using MiaCrate.Resources;
using OpenTK.Mathematics;

namespace MiaCrate.Client.Graphics;

public class PostChain : IDisposable
{
    private readonly IResourceManager _resourceManager;
    private readonly RenderTarget _screenTarget;
    private float _time;
    private float _lastStamp;
    private Matrix4 _shaderOrthoMatrix;
    private int _screenWidth;
    private int _screenHeight;
    private readonly string _name;
    private readonly List<PostPass> _passes = new();

    public PostChain(TextureManager textureManager, IResourceManager resourceManager, RenderTarget screenTarget,
        ResourceLocation location)
    {
        _resourceManager = resourceManager;
        _screenTarget = screenTarget;
        _time = 0f;
        _lastStamp = 0f;
        _screenWidth = screenTarget.ViewWidth;
        _screenHeight = screenTarget.ViewHeight;
        _name = location.ToString();
        
        UpdateOrthoMatrix();
        Load(textureManager, location);
    }

    private void UpdateOrthoMatrix()
    {
        _shaderOrthoMatrix = Matrix4.CreateOrthographic(_screenTarget.Width, _screenTarget.Height, 0.1f, 1000f);
    }

    public void Resize(int width, int height)
    {
        _screenWidth = _screenTarget.Width;
        _screenHeight = _screenTarget.Height;
        UpdateOrthoMatrix();
        
        foreach (var pass in _passes)
        {
            
        }
    }

    private void Load(TextureManager manager, ResourceLocation location)
    {
        
    }

    public void Dispose()
    {
    }
}