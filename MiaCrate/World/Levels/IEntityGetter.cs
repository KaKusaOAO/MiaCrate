using MiaCrate.World.Entities;
using MiaCrate.World.Phys;

namespace MiaCrate.World;

public interface IEntityGetter
{
    public List<Entity> GetEntities(Entity? entity, AABB aabb, Predicate<Entity> predicate);
}