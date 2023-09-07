namespace MiaCrate.Resources;

public class Pack
{
    private readonly Func<string, IPackResources> _resources;
    public string Id { get; }
    public bool IsRequired { get; }
    public bool IsFixedPosition { get; }
    public PackPosition DefaultPosition { get; }
    
    public IPackResources Open() => _resources(Id);
}