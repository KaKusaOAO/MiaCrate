namespace MiaCrate.Data;

public abstract class DataFix
{
    public bool ChangesType { get; }
    public Schema OutputSchema { get; }
    public Schema? InputSchema => ChangesType ? OutputSchema.Parent : OutputSchema;
    public int VersionKey => OutputSchema.VersionKey;

    public DataFix(Schema outputSchema, bool changesType)
    {
        OutputSchema = outputSchema;
        ChangesType = changesType;
    }
}