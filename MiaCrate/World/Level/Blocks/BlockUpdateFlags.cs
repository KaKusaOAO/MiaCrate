namespace MiaCrate.World.Blocks;

[Flags]
public enum BlockUpdateFlags
{
    Neighbors = 1 << 0,
    Clients = 1 << 1,
    Invisible = 1 << 2,
    Immediate = 1 << 3,
    KnownShape = 1 << 4,
    SuppressDrops = 1 << 5,
    MoveByPiston = 1 << 6,
    
    None = Invisible,
    All = Neighbors | Clients,
    AllImmediate = All | Immediate
}