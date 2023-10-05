using MiaCrate.Server.Levels;

namespace MiaCrate.World;

public interface ICustomSpawner
{
    public int Tick(ServerLevel level, bool bl, bool bl2);
}