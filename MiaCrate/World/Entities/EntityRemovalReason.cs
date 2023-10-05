namespace MiaCrate.World.Entities;

public enum EntityRemovalReason
{
    // @formatter:off
    Killed             = 1 << 4 | 0b10,
    Discarded          = 2 << 4 | 0b10,
    UnloadedToChunk    = 3 << 4 | 0b01,
    UnloadedWithPlayer = 4 << 4 | 0b00,
    ChangedDimension   = 5 << 4 | 0b00
    // @formatter:on
}

public static class EntityRemovalReasonExtension
{
    // @formatter:off
    private const int DestroyFlag = 0b10;
    private const int SaveFlag    = 0b01;
    // @formatter:on
    
    public static bool ShouldDestroy(this EntityRemovalReason reason) => 
        ((int) reason & DestroyFlag) == DestroyFlag;
    
    public static bool ShouldSave(this EntityRemovalReason reason) => 
        ((int) reason & SaveFlag) == SaveFlag;
}