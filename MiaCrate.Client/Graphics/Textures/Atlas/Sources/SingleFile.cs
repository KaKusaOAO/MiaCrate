using MiaCrate.Data;
using MiaCrate.Data.Codecs;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Client.Graphics;

public class SingleFile : ISpriteSource
{
    public static readonly ICodec<SingleFile> Codec =
        RecordCodecBuilder.Create<SingleFile>(instance => instance
            .Group(
                ResourceLocation.Codec.FieldOf("resource").ForGetter<SingleFile>(f => f._resourceId),
                ResourceLocation.Codec.OptionalFieldOf("sprite").ForGetter<SingleFile>(f => f._spriteId)
            )
            .Apply(instance, (location, optional) => new SingleFile(location, optional))
        );

    private readonly ResourceLocation _resourceId;
    private readonly IOptional<ResourceLocation> _spriteId;

    public SingleFile(ResourceLocation resourceId, IOptional<ResourceLocation> spriteId)
    {
        _resourceId = resourceId;
        _spriteId = spriteId;
    }

    public SpriteSourceType Type => SpriteSources.SingleFile;

    public void Run(IResourceManager manager, ISpriteSource.IOutput output)
    {
        var location = ISpriteSource.TextureIdConverter.IdToFile(_resourceId);
        var optional = manager.GetResource(location);
        if (optional.IsPresent)
        {
            output.Add(_spriteId.OrElse(_resourceId), optional.Value);
        }
        else
        {
            Logger.Warn($"Missing sprite: {location}");
        }
    }
}