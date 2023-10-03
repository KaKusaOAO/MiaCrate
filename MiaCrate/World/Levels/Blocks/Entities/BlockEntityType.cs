using System.Diagnostics.CodeAnalysis;
using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.DataFixes;
using Mochi.Utils;

namespace MiaCrate.World.Blocks;

public static partial class BlockEntityType
{
    private static BlockEntityType<T> Register
        <[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
        (string name, BlockEntityType<T>.Builder builder) where T : BlockEntity
    {
        if (!builder.ValidBlocks.Any())
        {
            Logger.Warn($"Block entity type {name} required at least one valid block to be defined!");
        }

        var dataType = Util.FetchChoiceType(References.BlockEntity, name);
        var type = builder.Build(dataType);
        
        var registered = Registry.Register(BuiltinRegistries.BlockEntityType, name, type);
        if (type == registered) return type;
        
        Logger.Warn($"Registered BlockEntityType not equal to the passed type? {type} != {registered}");
        return (BlockEntityType<T>) registered;
    }
}

public interface IBlockEntityType : IBuiltinRegistryEntryWithHolder<IBlockEntityType>
{
    public BlockEntity? Create(BlockPos pos, BlockState state);
}

public interface IBlockEntityType<out T> : IBlockEntityType where T : BlockEntity
{
    public new T? Create(BlockPos pos, BlockState state);
    BlockEntity? IBlockEntityType.Create(BlockPos pos, BlockState state) => Create(pos, state);
}

public delegate T? BlockEntityFactoryDelegate<out T>(BlockPos pos, BlockState state) where T : BlockEntity;

public class BlockEntityType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>
    : IBlockEntityType<T> where T : BlockEntity
{
    private readonly BlockEntityFactoryDelegate<T> _factory;
    private readonly HashSet<Block> _validBlocks;
    private readonly IType? _dataType;

    public IReferenceHolder<IBlockEntityType> BuiltinRegistryHolder { get; }

    public BlockEntityType(BlockEntityFactoryDelegate<T> factory, HashSet<Block> validBlocks, IType? dataType)
    {
        BuiltinRegistryHolder = BuiltinRegistries.BlockEntityType.CreateIntrusiveHolder(this);
        
        _factory = factory;
        _validBlocks = validBlocks;
        _dataType = dataType;
    }

    public class Builder
    {
        private readonly BlockEntityFactoryDelegate<T> _factory;
        internal HashSet<Block> ValidBlocks { get; }

        private Builder(BlockEntityFactoryDelegate<T> factory, HashSet<Block> validBlocks)
        {
            _factory = factory;
            ValidBlocks = validBlocks;
        }

        public static Builder Of(params Block[] blocks)
        {
            var ctor = typeof(T).GetConstructor(new[]
            {
                typeof(BlockPos), typeof(BlockState)
            });

            if (ctor == null)
                throw new Exception($"Type {typeof(T)} doesn't contain a standard constructor of a block entity type");
            
            var factory = new BlockEntityFactoryDelegate<T>((t, l) => (T) ctor.Invoke(new object[] {t, l}));
            return new Builder(factory, blocks.ToHashSet());
        }
        
        public static Builder Of(BlockEntityFactoryDelegate<T> factory, params Block[] blocks) => new(factory, blocks.ToHashSet());

        public BlockEntityType<T> Build(IType? type)
        {
            return new BlockEntityType<T>(_factory, ValidBlocks, type);
        }
    }

    public T? Create(BlockPos pos, BlockState state) => _factory(pos, state);
}