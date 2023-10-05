using MiaCrate.World.SaveData;

namespace MiaCrate.World.Entities;

public class Raids : SavedData
{
    private const string RaidFileId = "raids";

    public static bool CanJoinRaid(Raider? raider, Raid? raid)
    {
        if (raider == null || raid == null || raid.Level == null!) return false;
        throw new NotImplementedException();
    }
    
    
}