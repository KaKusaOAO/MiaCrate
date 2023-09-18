using System.Text.Json;
using System.Text.Json.Nodes;
using MiaCrate.Data;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class SpriteSourceList
{
    private static readonly FileToIdConverter _atlasInfoConverter = new("atlases", ".json");
    private readonly List<ISpriteSource> _sources;

    public SpriteSourceList(List<ISpriteSource> sources)
    {
        _sources = sources;
    }
    
    public List<Func<ISpriteResourceLoader, SpriteContents?>> List(IResourceManager manager)
    {
        var dict = new Dictionary<ResourceLocation, ISpriteSource.ISpriteSupplier>();
        var output = new Output(this, dict);
        foreach (var spriteSource in _sources)
        {
            spriteSource.Run(manager, output);
        }

        var list = new List<Func<ISpriteResourceLoader, SpriteContents?>>
        {
            _ => MissingTextureAtlasSprite.Create()
        };
        
        list.AddRange(dict.Values
            .Select(r => (Func<ISpriteResourceLoader, SpriteContents?>) r.Apply));
        return list;
    }

    public static SpriteSourceList Load(IResourceManager manager, ResourceLocation location)
    {
        var l = _atlasInfoConverter.IdToFile(location);
        var list = new List<ISpriteSource>();
        
        foreach (var resource in manager.GetResourceStack(l))
        {
            try
            {
                using var stream = resource.Open();
                var dyn = new Dynamic<JsonNode>(JsonOps.Instance, JsonSerializer.Deserialize<JsonNode>(stream));
                list.AddRange(SpriteSources.FileCodec.Parse(dyn)
                    .GetOrThrow(false, s => Logger.Error(s))
                );
            }
            catch (Exception ex)
            {
                Logger.Warn($"Failed to parse atlas definition {l} in pack {resource.SourcePackId}");
                Logger.Warn(ex);
            }
        }
        
        return new SpriteSourceList(list);
    }

    private class Output : ISpriteSource.IOutput
    {
        private readonly SpriteSourceList _instance;
        private readonly Dictionary<ResourceLocation, ISpriteSource.ISpriteSupplier> _dict;

        public Output(SpriteSourceList instance, Dictionary<ResourceLocation, ISpriteSource.ISpriteSupplier> dict)
        {
            _instance = instance;
            _dict = dict;
        }

        public void Add(ResourceLocation location, ISpriteSource.ISpriteSupplier supplier)
        {
            var other = _dict.AddOrSet(location, supplier);
            other?.Discard();
        }

        public void RemoveAll(Predicate<ResourceLocation> predicate)
        {
            var removal = new List<ResourceLocation>();
            
            foreach (var (key, value) in _dict)
            {
                if (predicate(key))
                {
                    value.Discard();
                    removal.Add(key);
                }
            }
            
            foreach (var resourceLocation in removal)
            {
                _dict.Remove(resourceLocation);
            }
        }
    }
}