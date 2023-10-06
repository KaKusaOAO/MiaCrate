namespace MiaCrate.World.Entities;

public class EntityDimensions
{
    public float Width { get; }
    public float Height { get; }
    public bool IsFixed { get; }
    
    public EntityDimensions(float width, float height, bool isFixed)
    {
        Width = width;
        Height = height;
        IsFixed = isFixed;
    }

    public static EntityDimensions CreateScalable(float width, float height) => new(width, height, false);
}