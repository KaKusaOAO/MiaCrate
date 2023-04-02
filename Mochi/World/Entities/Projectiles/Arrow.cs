namespace Mochi.World.Entities.Projectiles;

public class Arrow : AbstractArrow
{
    public Arrow(IEntityType type, ILevel level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        // throw new NotImplementedException();
    }
}