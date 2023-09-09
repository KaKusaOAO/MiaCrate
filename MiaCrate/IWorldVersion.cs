using MiaCrate.Resources;

namespace MiaCrate;

public interface IWorldVersion
{
	public DataVersion DataVersion { get; }
	public string Id { get; }
	public string Name { get; }
	public int ProtocolVersion { get; }
	public int GetPackVersion(PackType type);
	public DateTimeOffset BuildTime { get; }
	public bool IsStable { get; }
}