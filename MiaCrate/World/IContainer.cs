namespace MiaCrate.World;

public interface IContainer
{
    public const int LargeMaxStackSize = 64;
    public const int DefaultDistanceLimit = 8;
    
    int ContainerSize { get; }
    bool IsEmpty { get; }
}