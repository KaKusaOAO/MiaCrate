namespace MiaCrate.World;

public sealed class TerrainAdjustment : IStringRepresentable
{
    public string SerializedName { get;  }

    private TerrainAdjustment(string id)
    {
        SerializedName = id;
    }
}