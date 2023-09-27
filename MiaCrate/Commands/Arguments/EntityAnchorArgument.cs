using MiaCrate.World.Entities;
using MiaCrate.World.Phys;
using Mochi.Brigadier.Arguments;
using StringReader = Mochi.Brigadier.StringReader;

namespace MiaCrate.Commands.Arguments;

public class EntityAnchorArgument : IArgumentType<EntityAnchor>
{
    public EntityAnchor Parse(StringReader reader)
    {
        throw new NotImplementedException();
    }
}

public sealed class EntityAnchor : IEnumLike<EntityAnchor>
{
    private static readonly Dictionary<int, EntityAnchor> _values = new();

    private readonly string _name;
    private readonly Func<Vec3, Entity, Vec3> _transform;

    public static EntityAnchor Feet { get; } = new("feet", (v, _) => v);
    public static EntityAnchor Eyes { get; } = new("eyes", (v, e) => v + new Vec3(0, e.EyeHeight, 0));
    
    public int Ordinal { get; }

    public static EntityAnchor[] Values => _values.Values.ToArray();

    public Vec3 Apply(Entity entity) => _transform(entity.Position, entity);

    public Vec3 Apply(CommandSourceStack stack)
    {
        var entity = stack.Entity;
        return entity == null ? stack.Position : _transform(stack.Position, entity);
    }

    private EntityAnchor(string name, Func<Vec3, Entity, Vec3> transform)
    {
        _name = name;
        _transform = transform;

        Ordinal = _values.Count;
        _values[Ordinal] = this;
    }
}