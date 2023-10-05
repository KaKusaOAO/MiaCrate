namespace MiaCrate.World.Entities;

public interface INeutralMob
{
    public const string TagAngerTime = "AngerTime";
    public const string TagAngryAt = "AngryAt";
    
    public int RemainingPersistentAngerTime { get; set; }
    public Uuid? PersistentAngerTarget { get; set; }
    public void StartPersistentAngerTimer();
}