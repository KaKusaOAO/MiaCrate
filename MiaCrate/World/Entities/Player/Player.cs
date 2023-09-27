using MiaCrate.Auth;
using MiaCrate.Core;

namespace MiaCrate.World.Entities;

public abstract class Player : LivingEntity
{
    protected Player(Level level, BlockPos pos, float f, GameProfile profile) 
        : base(EntityType.Player, level)
    {
        throw new NotImplementedException();
    }
}