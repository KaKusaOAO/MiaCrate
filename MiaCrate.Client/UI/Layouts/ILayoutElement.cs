namespace MiaCrate.Client.UI;

public interface ILayoutElement
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; }
    public int Height { get; }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }
}