namespace Mochi.World.Entities;

public interface IOwnableEntity
{
    public Guid OwnerId { get; }
    public Entity Owner { get; }
}