using MiaCrate.Client.Graphics;
using MiaCrate.Common;
using MiaCrate.Extensions;
using MiaCrate.Resources;

namespace MiaCrate.Client.Resources;

public class AtlasSet : IDisposable
{
    private readonly Dictionary<ResourceLocation, AtlasEntry> _atlases = new();

    public AtlasSet(Dictionary<ResourceLocation, ResourceLocation> dict, TextureManager textureManager)
    {
        _atlases = dict.ToDictionary(
            e => e.Key,
            e =>
            {
                var atlas = new TextureAtlas(e.Key);
                textureManager.Register(e.Key, atlas);
                return new AtlasEntry(atlas, e.Value);
            });
    }
    
    public Dictionary<ResourceLocation, Task<StitchResult>> ScheduleLoad(IResourceManager manager, int i,
        IExecutor executor)
    {
        return _atlases.ToDictionary(e => e.Key, e =>
        {
            var entry = e.Value;
            return SpriteLoader.Create(entry.Atlas)
                .LoadAndStitchAsync(manager, entry.AtlasInfoLocation, i, executor)
                .ThenApplyAsync(p => new StitchResult(entry.Atlas, p));
        });
    }

    public void Dispose()
    {
    }

    public record AtlasEntry(TextureAtlas Atlas, ResourceLocation AtlasInfoLocation) : IDisposable
    {
        public void Dispose()
        {
            Atlas.ClearTextureData();
        }
    }

    public class StitchResult
    {
        private readonly TextureAtlas _atlas;
        private readonly SpriteLoader.Preparations _preparations;

        public TextureAtlasSprite Missing => _preparations.Missing;
        public Task ReadyForUpload => _preparations.ReadyForUpload;

        public StitchResult(TextureAtlas atlas, SpriteLoader.Preparations preparations)
        {
            _atlas = atlas;
            _preparations = preparations;
        }

        public void Upload()
        {
            
        }

        public TextureAtlasSprite? GetSprite(ResourceLocation location) => 
            _preparations.Regions.GetValueOrDefault(location);
    }
}