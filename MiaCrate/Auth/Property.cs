namespace MiaCrate.Auth;

public class Property
{
    public string Name { get; }
    public string Value { get; }
    public string? Signature { get; }
    
    public Property(string name, string value, string? signature = null)
    {
        Name = name;
        Value = value;
        Signature = signature;
    }

    public bool HasSignature => Signature != null;
    
    
}