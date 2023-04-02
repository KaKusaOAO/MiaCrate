namespace MiaCrate.World.Entities.AI;

public class Attribute
{
    public const int MaxNameLength = 64;
    private readonly double _defaultValue;
    private bool _syncable;
    private readonly string _descriptionId;

    protected Attribute(string descriptionId, double defaultValue)
    {
        _defaultValue = defaultValue;
        _descriptionId = descriptionId;
    }

    public double GetDefaultValue()
    {
        return _defaultValue;
    }

    public bool IsClientSyncable()
    {
        return _syncable;
    }

    public Attribute SetSyncable(bool syncable)
    {
        _syncable = syncable;
        return this;
    }

    public virtual double SanitizeValue(double value)
    {
        return value;
    }

    public string GetDescriptionId()
    {
        return _descriptionId;
    }
}