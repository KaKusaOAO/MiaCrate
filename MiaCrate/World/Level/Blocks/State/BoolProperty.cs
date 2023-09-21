using MiaCrate.Data;
using Mochi.Utils;

namespace MiaCrate.World.Blocks;

#pragma warning disable CS0659
public class BoolProperty : Property<bool>
{
    private static readonly HashSet<bool> _values = new() {true, false};

    protected BoolProperty(string name) : base(name)
    {
    }

    public static BoolProperty Create(string name) => new(name);

    public override List<bool> PossibleValues => _values.ToList();

    public override string GetName(bool value) => value.ToString().ToLowerInvariant();

    public override IOptional<bool> GetValue(string serialized)
    {
        return serialized switch
        {
            "true" => Optional.Of(true),
            "false" => Optional.Of(false),
            _ => Optional.Empty<bool>()
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BoolProperty other) return false;
        return base.Equals(other);
    }

    public override int GenerateHashCode() => 31 * base.GenerateHashCode() * _values.GetHashCode();
}