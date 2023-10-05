using MiaCrate.Auth;
using MiaCrate.Core;

namespace MiaCrate.World.Entities;

public abstract class Player : LivingEntity
{
    public Abilities Abilities { get; } = new();
    
    protected Player(Level level, BlockPos pos, float f, GameProfile profile) 
        : base(EntityType.Player, level)
    {
        throw new NotImplementedException();
    }
}