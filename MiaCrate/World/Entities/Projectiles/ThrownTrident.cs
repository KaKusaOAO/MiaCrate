namespace MiaCrate.World.Entities.Projectiles;

public class ThrownTrident : AbstractArrow
{
    public ThrownTrident(IEntityType type, Level level) : base(type, level)
    {
    }

    protected override void DefineSynchedData()
    {
        throw new NotImplementedException();
    }
}