namespace MiaCrate.Client;

public sealed class CloudStatus : IEnumLike<CloudStatus>, IOptionEnum, IStringRepresentable
{
    private static readonly Dictionary<int, CloudStatus> _values = new();

    public static CloudStatus Off { get; } = new("false", "options.off");
    public static CloudStatus Fast { get; } = new("fast", "options.clouds.fast");
    public static CloudStatus Fancy { get; } = new("true", "options.clouds.fancy");

    public int Ordinal { get; }
    int IOptionEnum.Id => Ordinal;

    public static CloudStatus[] Values => _values.Values.ToArray();

    public string Key { get; }

    public string SerializedName { get; }

    private CloudStatus(string legacyName, string key)
    {
        SerializedName = legacyName;
        Key = key;

        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }
}