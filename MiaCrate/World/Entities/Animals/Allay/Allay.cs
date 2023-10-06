using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Net.Packets.Play;
using MiaCrate.World.Blocks;
using MiaCrate.World.Entities.AI;
using MiaCrate.World.Entities.AI.Memory;
using MiaCrate.World.Entities.AI.Sensing;

namespace MiaCrate.World.Entities;

public class Allay : PathfinderMob, IInventoryCarrier, IVibrationSystem
{
    private const int LiftingItemAnimationDuration = 5;
    private const float DancingLoopDuration = 55f;
    private const float SpinningAnimationDuration = 15f;
    private const int DuplicationCooldownTicks = 6000;
    private const int NumOfDuplicationHearts = 3;
    
    private static List<IMemoryModuleType> MemoryTypes { get; }
    private static List<ISensorType<ISensor<Allay>>> SensorTypes { get; }

    protected IBrain<Allay> AllayBrain => (IBrain<Allay>) Brain;

    protected IBrainProvider<Allay> AllayBrainProvider => IBrain.CreateProvider<Allay>(
        MemoryTypes,
        SensorTypes.Cast<ISensorType>().ToList());

    protected override IBrainProvider BrainProvider => AllayBrainProvider;

    public Allay(IEntityType type, Level level) : base(type, level) {}

    protected override IBrain MakeBrain(IDynamic dyn) => AllayAi.MakeBrain(AllayBrainProvider.MakeBrain(dyn));

    protected override void PlayStepSound(BlockPos pos, BlockState state) {}

    protected override void SendDebugPackets()
    {
        base.SendDebugPackets();
        DebugPackets.SendEntityBrain(this);
    }
}