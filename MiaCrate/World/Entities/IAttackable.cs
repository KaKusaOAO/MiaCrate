namespace MiaCrate.World.Entities;

public interface IAttackable
{
    public LivingEntity? LastAttacker { get; }
}