namespace MiaCrate.Auth;

public class GameProfile
{
    public Guid? Id { get; }
    public string? Name { get; }
    public PropertyMap PropertyMap { get; } = new();
    public bool IsLegacy { get; private set; }

    public GameProfile(Guid? id, string name)
    {
        if (id == null && name.IsBlank())
            throw new ArgumentException("Name and ID cannot both be blank");
        
        Id = id;
        Name = name;
    }

    public bool IsComplete => Id != null && !Name.IsBlank();

    public override int GetHashCode()
    {
        var result = Id.GetHashCode();
        result = 31 * result + (Name?.GetHashCode() ?? 0);
        return result;
    }

    public override bool Equals(object obj)
    {
        if (obj is not GameProfile other) return false;
        return Id == other.Id && Name == other.Name;
    }

    public static bool operator ==(GameProfile a, GameProfile b) => a.Equals(b);
    public static bool operator !=(GameProfile a, GameProfile b) => !a.Equals(b);
}