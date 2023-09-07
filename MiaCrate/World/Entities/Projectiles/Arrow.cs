namespace MiaCrate.World.Entities.Projectiles;

public class Arrow : AbstractArrow
{
    public Arrow(IEntityType type, Level level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        // throw new NotImplementedException();
    }
}