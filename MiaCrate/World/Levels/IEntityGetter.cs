using MiaCrate.World.Entities;
using MiaCrate.World.Phys;

namespace MiaCrate.World;

public interface IEntityGetter
{
    public List<Player> Players { get; }
    
    public List<Entity> GetEntities(Entity? entity, AABB aabb, Predicate<Entity> predicate);

    public Player? GetPlayerByUuid(Uuid uuid) => Players.FirstOrDefault(p => p.Uuid == uuid);
}