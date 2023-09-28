using MiaCrate.Client.Platform;
using MiaCrate.Client.Systems;
using MiaCrate.Resources;
using MiaCrate.World.Inventories;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class TextureAtlas : AbstractTexture, IDumpable, ITickable
{
    public static ResourceLocation LocationBlocks { get; } = InventoryMenu.BlockAtlas;
    public static ResourceLocation LocationParticles { get; } = new("textures/atlas/particles.png");

    private List<SpriteContents> _sprites = new();
    private List<TextureAtlasSprite.ITicker> _animatedTextures = new();
    private Dictionary<ResourceLocation, TextureAtlasSprite> _texturesByName = new();

    public ResourceLocation Location { get; }
    public int MaxSupportedTextureSize { get; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int MipLevel { get; private set; }

    public TextureAtlas(ResourceLocation location)
    {
        Location = location;
        MaxSupportedTextureSize = RenderSystem.MaxSupportedTextureSize;
    }
    
    public override void Load(IResourceManager manager)
    {
        // Seems intended to be empty
    }

    public void DumpContents(ResourceLocation location, string path)
    {
        Util.LogFoobar();
    }

    public void Tick()
    {
        RenderSystem.EnsureOnRenderThread(CycleAnimationFrames);
    }

    public void CycleAnimationFrames()
    {
        Bind();

        foreach (var texture in _animatedTextures)
        {
            texture.TickAndUpload();
        }
    }

    public TextureAtlasSprite GetSprite(ResourceLocation location)
    {
        return _texturesByName.GetValueOrDefault(location) ??
               _texturesByName[MissingTextureAtlasSprite.Location];
    }

    public void ClearTextureData()
    {
        foreach (var contents in _sprites)
        {
            contents.Dispose();
        }

        foreach (var texture in _animatedTextures)
        {
            texture.Dispose();
        }

        _sprites = new List<SpriteContents>();
        _animatedTextures = new List<TextureAtlasSprite.ITicker>();
        _texturesByName = new Dictionary<ResourceLocation, TextureAtlasSprite>();
    }

    public void Upload(SpriteLoader.Preparations preparations)
    {
        Logger.Info($"Created: {preparations.Width}x{preparations.Height}x{preparations.MipLevel} {Location}-atlas");
        var preparation = TextureUtil.PrepareImage(preparations.MipLevel, preparations.Width, preparations.Height);
        _textureDescription = preparation.TextureDescription;
        _samplerDescription = preparation.SamplerDescription;

        var factory = GlStateManager.ResourceFactory;
        Texture?.Dispose();
        Texture = factory.CreateTexture(_textureDescription);
        
        Sampler?.Dispose();
        Sampler = factory.CreateSampler(_samplerDescription);

        Width = preparations.Width;
        Height = preparations.Height;
        MipLevel = preparations.MipLevel;
        ClearTextureData();
        _texturesByName = preparations.Regions;

        var list = new List<SpriteContents>();
        var list2 = new List<TextureAtlasSprite.ITicker>();
        
        foreach (var sprite in preparations.Regions.Values)
        {
            list.Add(sprite.Contents);

            try
            {
                sprite.UploadFirstFrame(this);
            }
            catch (Exception ex)
            {
                var report = CrashReport.ForException(ex, "Stitching texture atlas");
                throw new ReportedException(report);
            }

            var ticker = sprite.CreateTicker();
            if (ticker != null)
            {
                list2.Add(ticker);
            }
        }

        _sprites = list;
        _animatedTextures = list2;
    }
}