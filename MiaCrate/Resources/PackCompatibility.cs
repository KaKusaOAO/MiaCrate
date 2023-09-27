using MiaCrate.Extensions;
using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.Resources;

public sealed class PackCompatibility
{
    public IComponent Description { get; }
    public IComponent Confirmation { get; }

    public static readonly PackCompatibility TooOld = new("old");
    public static readonly PackCompatibility TooNew = new("new");
    public static readonly PackCompatibility Compatible = new("compatible");
    
    public PackCompatibility(string type)
    {
        Description = TranslateText.Of($"pack.incompatible.{type}").WithColor(TextColor.Gray);
        Confirmation = TranslateText.Of($"pack.incompatible.confirm.{type}");
    }

    public bool IsCompatible => this == Compatible;

    public static PackCompatibility ForFormat(int format, PackType type)
    {
        var curr = SharedConstants.CurrentVersion.GetPackVersion(type);
        if (format < curr) return TooOld;
        return format > curr ? TooNew : Compatible;
    }
}