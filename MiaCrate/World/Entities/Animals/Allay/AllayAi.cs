using MiaCrate.World.Entities.AI;

namespace MiaCrate.World.Entities;

public static class AllayAi
{
    private const float SpeedMultiplierWhenIdling = 1;
    private const float SpeedMultiplierWhenFollowingDepositTarget = 2.25f;
    private const float SpeedMultiplierWhenRetrievingItem = 1.75f;
    private const float SpeedMultiplierWhenPanicking = 2.5f;

    private const int CloseEnoughToTarget = 4;
    private const int TooFarFromTarget = 16;
    private const int MaxLookDistance = 6;
    private const int MinWaitDuration = 30;
    private const int MaxWaitDuration = 60;
    private const int TimeToForgetNoteblock = 30 * SharedConstants.TicksPerSecond;
    private const int DistanceToWantedItem = 32;
    private const int GiveItemTimeoutDuration = 20;
    
    public static IBrain<Allay> MakeBrain(IBrain<Allay> brain)
    {
        InitCoreActivity(brain);
        return brain;
    }

    private static void InitCoreActivity(IBrain<Allay> brain)
    {
        throw new NotImplementedException();
    }
}