namespace MiaCrate;

public class DataVersion
{
	public int Version { get; }
	public string Series { get; }
	public const string MainSeries = "main";

	public DataVersion(int version) : this(version, MainSeries) { }

	public DataVersion(int version, string series)
	{
		Version = version;
		Series = series;
	}

	public bool IsSideSeries => Series == MainSeries;
}