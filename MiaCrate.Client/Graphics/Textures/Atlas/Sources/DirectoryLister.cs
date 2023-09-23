using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public class DirectoryLister : ISpriteSource
{
    public static ICodec<DirectoryLister> Codec { get; } =
        RecordCodecBuilder.Create<DirectoryLister>(instance => instance
            .Group(
                Data.Codec.String.FieldOf("source")
                    .ForGetter<DirectoryLister>(l => l.SourcePath),
                Data.Codec.String.FieldOf("prefix")
                    .ForGetter<DirectoryLister>(l => l.IdPrefix)
            )
            .Apply(instance, (s, s1) => new DirectoryLister(s, s1))
        );

    public string SourcePath { get; }
    public string IdPrefix { get; }
    
    public SpriteSourceType Type => SpriteSources.Directory;
    
    public DirectoryLister(string sourcePath, string idPrefix)
    {
        SourcePath = sourcePath;
        IdPrefix = idPrefix;
    }

    public void Run(IResourceManager manager, ISpriteSource.IOutput output)
    {
        var converter = new FileToIdConverter($"textures/{SourcePath}", ".png");
        foreach (var (key, value) in converter.ListMatchingResources(manager))
        {
            var l = converter.FileToId(key).WithPrefix(IdPrefix);
            output.Add(l, value);
        }
    }
}