using MiaCrate.Data;

namespace MiaCrate.DataFixes;

public class BlockEntitySignTextStrictJsonFix : NamedEntityFix
{
    public BlockEntitySignTextStrictJsonFix(Schema outputSchema, bool changesType) 
        : base(outputSchema, changesType, nameof(BlockEntitySignTextStrictJsonFix), References.BlockEntity, "Sign")
    {
    }
}