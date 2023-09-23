namespace MiaCrate;

public sealed class ResultField : IComparable<ResultField>
{
    public string Name { get; }
    public double Percentage { get; }
    public double GlobalPercentage { get; }
    public long Count { get; }

    public int Color => (Name.GetHashCode() & 0xaaaaaa) + 0x444444;
    
    public ResultField(string name, double percentage, double globalPercentage, long count)
    {
        Name = name;
        Percentage = percentage;
        GlobalPercentage = globalPercentage;
        Count = count;
    }

    public int CompareTo(ResultField? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;

        if (other.Percentage < Percentage) return -1;
        return other.Percentage > Percentage ? 1 : string.Compare(other.Name, Name, StringComparison.Ordinal);
    }
}