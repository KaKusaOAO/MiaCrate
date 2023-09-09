using MiaCrate.Texts;
using Mochi.Texts;
using Mochi.Utils;

namespace MiaCrate.Resources;

public class Pack
{
    private readonly Func<string, IPackResources> _resources;
    
    public IComponent Title { get; }
    public IComponent Description { get; }
    public FeatureFlagSet RequestedFeatures { get; }
    public PackCompatibility Compatibility { get; }
    public IPackSource PackSource { get; }
    public string Id { get; }
    public bool IsRequired { get; }
    public bool IsFixedPosition { get; }
    public PackPosition DefaultPosition { get; }

    private Pack(string id, bool required, Func<string, IPackResources> resources, IComponent title, PackInfo info,
        PackCompatibility compatibility, PackPosition position, bool fixedPosition, IPackSource source)
    {
        Id = id;
        _resources = resources;
        Title = title;
        Description = info.Description;
        Compatibility = compatibility;
        RequestedFeatures = info.RequestedFeatures;
        IsRequired = required;
        DefaultPosition = position;
        IsFixedPosition = fixedPosition;
        PackSource = source;
    }

    public IPackResources Open() => _resources(Id);
    
    public static Pack? ReadMetaAndCreate(string id, IComponent title, bool required,
        Func<string, IPackResources> resources, PackType type, PackPosition position, IPackSource source)
    {
        var info = ReadPackInfo(id, resources);
        return info != null ? Create(id, title, required, resources, info, type, position, false, source) : null;
    }

    public static Pack Create(string id, IComponent title, bool required, Func<string, IPackResources> resources, PackInfo info,
        PackType type, PackPosition position, bool fixedPosition, IPackSource source) =>
        new(id, required, resources, title, info, info.GetCompatibility(type), position, fixedPosition, source);

    public static PackInfo? ReadPackInfo(string name, Func<string, IPackResources> resourceSupplier)
    {
        try
        {
            using var resources = resourceSupplier(name);
            var metadata = resources.GetMetadataSection(PackMetadataSection.Type);
            if (metadata == null)
            {
                Logger.Warn($"Missing metadata in pack {name}");
                return null;
            }

            var flags = resources.GetMetadataSection(FeatureFlagsMetadataSection.Type);
            var set = flags?.Flags ?? FeatureFlagSet.Empty;
            return new PackInfo(metadata.Description, metadata.PackFormat, set);
        }
        catch (Exception ex)
        {
            Logger.Warn("Failed to read pack metadata");
            Logger.Warn(ex);
            return null;
        }
    }

    public override int GetHashCode() => Id.GetHashCode();
}