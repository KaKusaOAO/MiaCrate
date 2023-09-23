namespace MiaCrate.Client.Models;

public class MaterialDefinition
{
    public int XTexSize { get; }
    public int YTexSize { get; }
    
    public MaterialDefinition(int xTexSize, int yTexSize)
    {
        XTexSize = xTexSize;
        YTexSize = yTexSize;
    }
}