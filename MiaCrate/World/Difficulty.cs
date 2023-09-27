using MiaCrate.Texts;
using Mochi.Texts;

namespace MiaCrate.World;

public sealed class Difficulty : IEnumLike<Difficulty>, IStringRepresentable
{
    private static readonly Dictionary<int, Difficulty> _byId = new();

    public static Difficulty Peaceful { get; } = new(0, "peaceful");
    public static Difficulty Easy { get; } = new(1, "easy");
    public static Difficulty Normal { get; } = new(2, "normal");
    public static Difficulty Hard { get; } = new(3, "hard");

    public static IStringRepresentable.EnumCodec<Difficulty> Codec { get; } =
        IStringRepresentable.FromEnum<Difficulty>();

    public int Id { get; }
    public string Key { get; }
    
    int IEnumLike<Difficulty>.Ordinal => Id;
    string IStringRepresentable.SerializedName => Key;

    public IComponent DisplayName => MiaComponent.Translatable($"options.difficulty.{Key}");
    public IComponent Info => MiaComponent.Translatable($"options.difficulty.{Key}.info");

    public static Difficulty[] Values => _byId.Values.ToArray();

    private Difficulty(int id, string key)
    {
        Id = id;
        Key = key;
        
        _byId[id] = this;
    }

    public static Difficulty ById(int id) => _byId[Util.PositiveModulo(id, Values.Length)];

    public static Difficulty? ByName(string key) => Codec.ByName(key);
}