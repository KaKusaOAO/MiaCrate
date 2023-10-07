namespace MiaCrate.Client.UI;

public class SpacerElement : ILayoutElement
{
    public int X { get; set; }

    public int Y { get; set; }

    public int Width { get; }

    public int Height { get; }

    public SpacerElement(int width, int height) : this(0, 0, width, height) {}
    
    public SpacerElement(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static SpacerElement CreateWidth(int x) => new(x, 0);
    public static SpacerElement CreateHeight(int y) => new(0, y);
    
    public void VisitWidgets(Action<AbstractWidget> consumer)
    {
        
    }
}