namespace MiaCrate.Resources;

public class FeatureFlagUniverse
{
    private readonly string _id;

    public FeatureFlagUniverse(string id)
    {
        _id = id;
    }

    public override string ToString() => _id;
}