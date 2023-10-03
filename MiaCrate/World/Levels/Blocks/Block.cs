using MiaCrate.Core;
using MiaCrate.World.Items;
using Mochi.Utils;

namespace MiaCrate.World.Blocks;

public partial class Block : BlockBehavior, IItemLike, IBuiltinRegistryEntryWithHolder<Block>
{
    public const float Indestructible = -1f;
    public const float Instant = 0f;
    public const int UpdateLimit = 512;
    public const int CacheSize = 2048;

    public IReferenceHolder<Block> BuiltinRegistryHolder { get; }

    public IStateDefinition<Block, BlockState> StateDefinition { get; }
    public BlockState DefaultBlockState { get; protected set; }

    public Block(BlockProperties properties) : base(properties)
    {
        BuiltinRegistryHolder = BuiltinRegistries.Block.CreateIntrusiveHolder(this);

        var builder = new StateDefinition<Block, BlockState>.Builder(this);
        CreateBlockStateDefinition(builder);
        StateDefinition = builder.Create(b => b.DefaultBlockState, BlockState.CreateFactory());
        DefaultBlockState = StateDefinition.Any();

        if (!SharedConstants.IsRunningInIde) return;
        
        var name = GetType().Name;
        if (name.EndsWith("Block")) return;
        
        Logger.Error($"Block classes should end with Block and {name} doesn't.");
    }

    protected virtual void CreateBlockStateDefinition(StateDefinition<Block, BlockState>.Builder builder)
    {
        
    }

    public virtual void AnimateTick(BlockState state, Level level, BlockPos pos, IRandomSource randomSource)
    {
        
    }

    public Item AsItem()
    {
        throw new NotImplementedException();
    }
}