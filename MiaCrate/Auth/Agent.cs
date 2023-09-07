namespace MiaCrate.Auth;

public class Agent
{
    public string Name { get; }
    public int Version { get; }
    
    public static readonly Agent Minecraft = new("Minecraft", 1);
    public static readonly Agent Scrolls = new("Scrolls", 1);

    public Agent(string name, int version)
    {
        Name = name;
        Version = version;
    }

    public override string ToString() => $"Agent{{name='{Name}', version={Version}}}";
}