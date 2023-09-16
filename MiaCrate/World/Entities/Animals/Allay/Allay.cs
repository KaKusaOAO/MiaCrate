namespace MiaCrate.World.Entities;

public class Allay : PathfinderMob, IInventoryCarrier, IVibrationSystem
{
    private const int LiftingItemAnimationDuration = 5;
    private const float DancingLoopDuration = 55f;
    private const float SpinningAnimationDuration = 15f;
    private const int DuplicationCooldownTicks = 6000;
    private const int NumOfDuplicationHearts = 3;
    
    public Allay(IEntityType type, Level level) : base(type, level) {}
}