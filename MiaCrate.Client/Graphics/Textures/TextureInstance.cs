using MiaCrate.Client.Platform;
using MiaCrate.Extensions;
using Veldrid;

namespace MiaCrate.Client.Graphics;

public class TextureInstance : IDisposable
{
    private readonly Dictionary<TextureDescription, Texture> _textureCache = new();
    private readonly Dictionary<SamplerDescription, Sampler> _samplerCache = new();
    private readonly Dictionary<TextureViewDescription, TextureView> _texViewCache = new();

    public delegate void TextureModifier(ref TextureDescription desc);
    public delegate void SamplerModifier(ref SamplerDescription desc);
    
    private TextureDescription _textureDescription;
    private SamplerDescription _samplerDescription;
    
    private bool _textureDirty;
    private bool _samplerDirty;
    
    private Texture? _texture;
    private TextureView? _textureView;
    private Sampler? _sampler;

    private string _name = "";
    private bool _nameChanged;
    
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            _nameChanged = true;
            UpdateNames();
        } 
    }

    public Texture Texture => GetOrCreateTexture();
    public Sampler Sampler => GetOrCreateSampler();
    public TextureView TextureView => GetOrCreateTextureView();

    public TextureInstance()
    {
        _textureDescription = new TextureDescription();
        _samplerDescription = new SamplerDescription();
    }
    
    public TextureInstance(TextureDescription textureDescription, SamplerDescription samplerDescription)
    {
        _textureDescription = textureDescription;
        _samplerDescription = samplerDescription;
    }

    public void UpdateTextureDescription(TextureDescription desc)
    {
        var t = _textureDescription;
        _textureDescription = desc;
        
        if (!t.Equals(_textureDescription))
            _textureDirty = true;
    }
    
    public void UpdateSamplerDescription(SamplerDescription desc)
    {
        var s = _samplerDescription;
        _samplerDescription = desc;

        if (!s.Equals(_samplerDescription))
            _samplerDirty = true;
    }

    public void ModifyTexture(TextureModifier modifier)
    {
        var t = _textureDescription;
        modifier(ref _textureDescription);

        if (!t.Equals(_textureDescription))
            _textureDirty = true;
    }

    public void ModifySampler(SamplerModifier modifier)
    {
        var s = _samplerDescription;
        modifier(ref _samplerDescription);

        if (!s.Equals(_samplerDescription))
            _samplerDirty = true;
    }

    private Texture GetOrCreateTexture()
    {
        if (_texture == null || _texture.IsDisposed) EnsureTextureUpToDate();
        return _texture!;
    }
    
    public void EnsureTextureViewUpToDate()
    {
        if (!Texture.Usage.HasFlag(TextureUsage.Sampled) && !Texture.Usage.HasFlag(TextureUsage.Storage))
            return;
        
        var desc = new TextureViewDescription(Texture);
        
        _textureView = _texViewCache.ComputeIfAbsent(desc,
            d => GlStateManager.ResourceFactory.CreateTextureView(d));

        if (_textureView.IsDisposed)
        {
            // Recreate the resource as it is disposed
            _textureView = _texViewCache[desc] = 
                GlStateManager.ResourceFactory.CreateTextureView(desc);
        }
        
        UpdateNames();
    }
    
    private TextureView GetOrCreateTextureView()
    {
        if (_texture == null || _texture.IsDisposed || _textureDirty) EnsureTextureUpToDate();
        
        if (!Texture.Usage.HasFlag(TextureUsage.Sampled) && !Texture.Usage.HasFlag(TextureUsage.Storage))
            throw new InvalidOperationException("The specified texture cannot be used to create a texture view");
        
        if (_textureView == null || _textureView.IsDisposed) EnsureTextureViewUpToDate();
        return _textureView!;
    }
    
    private Sampler GetOrCreateSampler()
    {
        if (_sampler == null || _sampler.IsDisposed) EnsureSamplerUpToDate();
        return _sampler!;
    }
    
    public void EnsureSamplerUpToDate()
    {
        _sampler = _samplerCache.ComputeIfAbsent(_samplerDescription,
            d => GlStateManager.ResourceFactory.CreateSampler(d));
        
        if (_sampler.IsDisposed)
        {
            // Recreate the resource as it is disposed
            _sampler = _samplerCache[_samplerDescription] = 
                GlStateManager.ResourceFactory.CreateSampler(_samplerDescription);
        }
        
        UpdateNames();
        _samplerDirty = false;
    }

    private void UpdateNames()
    {
        if (!_nameChanged) return;
        Texture.Name = $"Texture - {_name}";
        Sampler.Name = $"Sampler - {_name}";
        TextureView.Name = $"TextureView - {_name}";
    }

    public void EnsureUpToDate()
    {
        EnsureTextureUpToDate();
        EnsureSamplerUpToDate();
        EnsureTextureViewUpToDate();
    }
    
    public void EnsureTextureUpToDate()
    {
        _texture = _textureCache.ComputeIfAbsent(_textureDescription,
            d => GlStateManager.ResourceFactory.CreateTexture(d));

        if (_texture.IsDisposed)
        {
            // Recreate the resource as it is disposed
            _texture = _textureCache[_textureDescription] = 
                GlStateManager.ResourceFactory.CreateTexture(_textureDescription);
        }
        
        UpdateNames();
        _textureDirty = false;
    }

    public void Dispose()
    {
        _texture?.Dispose();
        _sampler?.Dispose();
    }
}