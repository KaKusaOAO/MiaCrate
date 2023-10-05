namespace MiaCrate.Server.Levels;

public class ChunkHolder
{
    
}

public abstract class ChunkLoadingFailure
{
    public static ChunkLoadingFailure Unloaded { get; } = new UnloadedLoadingFailure();
    
    private class UnloadedLoadingFailure : ChunkLoadingFailure
    {
        public override string ToString() => "UNLOADED";
    }
}
