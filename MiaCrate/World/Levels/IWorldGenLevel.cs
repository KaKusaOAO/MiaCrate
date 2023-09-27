using MiaCrate.Core;

namespace MiaCrate.World;

public interface IWorldGenLevel
{
    public long Seed { get; }

    public bool EnsureCanWrite(BlockPos pos) => true;

    public void SetCurrentlyGenerating(Func<string>? currentlyGenerating);
}