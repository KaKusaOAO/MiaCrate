using MiaCrate.Client.Resources;
using MiaCrate.Extensions;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class SpriteLoader
{
    public static readonly HashSet<IMetadataSectionSerializer> DefaultMetadataSections = 
        new() {AnimationMetadataSection.Serializer};
    
    private readonly ResourceLocation _location;
    private readonly int _maxSupportedTextureSize;
    private readonly int _minWidth;
    private readonly int _minHeight;

    public SpriteLoader(ResourceLocation location, int maxSupportedTextureSize, int minWidth, int minHeight)
    {
        _location = location;
        _maxSupportedTextureSize = maxSupportedTextureSize;
        _minWidth = minWidth;
        _minHeight = minHeight;
    }
    
    public static SpriteLoader Create(TextureAtlas textureAtlas) =>
        new(textureAtlas.Location, textureAtlas.MaxSupportedTextureSize, textureAtlas.Width, textureAtlas.Height);
    
    public static Stitcher<T>.ISpriteLoader CreateWithEntry<T>(Action<T, int, int> action) where T : IStitcherEntry =>
        Stitcher<T>.ISpriteLoader.Create(action);

    public Preparations Stitch(List<SpriteContents> list, int i, IExecutor executor)
    {
        var j = _maxSupportedTextureSize;
        var stitcher = new Stitcher<SpriteContents>(j, j, i);

        var k = int.MaxValue;
        var l = 1 << i;

        int m;
        foreach (var spriteContents in list)
        {
            k = Math.Min(k, Math.Min(spriteContents.Width, spriteContents.Height));
            m = Math.Min(Util.LowestOneBit(spriteContents.Width), Util.LowestOneBit(spriteContents.Height));

            if (m < l)
            {
                Logger.Warn($"Texture {spriteContents.Name} with size " +
                            $"{spriteContents.Width}x{spriteContents.Height} limits mip level from " +
                            $"{Util.Log2(l)} to {Util.Log2(l)}");
                l = m;
            }

            stitcher.RegisterSprite(spriteContents);
        }

        var n = Math.Min(k, l);
        var o = Util.Log2(n);
        if (o < i)
        {
            Logger.Warn($"{_location}: dropping miplevel from {i} to {o}, because of minimum power of two: {n}");
            m = o;
        }
        else
        {
            m = i;
        }

        try
        {
            stitcher.Stitch();
        }
        catch (StitcherException ex)
        {
            var report = CrashReport.ForException(ex, "Stitching");
            throw new ReportedException(report);
        }

        var p = Math.Max(stitcher.Width, _minWidth);
        var q = Math.Max(stitcher.Height, _minHeight);
        var dict = GetStitchedSprites(stitcher, p, q);
        var missingSprite = dict[MissingTextureAtlasSprite.Location];

        Task task;
        if (m > 0)
        {
            task = Tasks.RunAsync(() =>
            {
                foreach (var textureAtlasSprite in dict.Values)
                {
                    textureAtlasSprite.Contents.IncreaseMipLevel(m);
                }
            }, executor);
        }
        else
        {
            task = Task.CompletedTask;
        }

        return new Preparations(p, q, m, missingSprite, dict, task);
    }

    private Dictionary<ResourceLocation, TextureAtlasSprite> GetStitchedSprites(Stitcher<SpriteContents> stitcher,
        int width, int height)
    {
        var dict = new Dictionary<ResourceLocation, TextureAtlasSprite>();
        stitcher.GatherSprites(CreateWithEntry<SpriteContents>((contents, k, l) =>
        {
            dict[contents.Name] = new TextureAtlasSprite(_location, contents, width, height, k, l);
        }));
        
        return dict;
    }

    public Task<Preparations> LoadAndStitchAsync(IResourceManager manager, ResourceLocation location, int i,
        IExecutor executor) => 
        LoadAndStitchAsync(manager, location, i, executor, DefaultMetadataSections);

    public Task<Preparations> LoadAndStitchAsync(IResourceManager manager, ResourceLocation location, int i,
        IExecutor executor, IEnumerable<IMetadataSectionSerializer> serializers)
    {
        var loader = ISpriteResourceLoader.Create(serializers);
        return Tasks.SupplyAsync(() => SpriteSourceList.Load(manager, location).List(manager), executor)
            .ThenComposeAsync(list => RunSpriteSuppliers(loader, list, executor))
            .ThenApplyAsync(list => Stitch(list, i, executor));
    }

    public static Task<List<SpriteContents>> RunSpriteSuppliers(ISpriteResourceLoader loader,
        List<Func<ISpriteResourceLoader, SpriteContents?>> list, IExecutor executor)
    {
        var result = list
            .Select(func => Tasks.SupplyAsync(() => func(loader), executor))
            .ToList();

        return Util.Sequence(result)
            .ThenApplyAsync(l => l
                .Where(o => o != null)
                .Cast<SpriteContents>()
                .ToList());
    }

    public record Preparations(int Width, int Height, int MipLevel, TextureAtlasSprite Missing,
        Dictionary<ResourceLocation, TextureAtlasSprite> Regions, Task ReadyForUpload);
}