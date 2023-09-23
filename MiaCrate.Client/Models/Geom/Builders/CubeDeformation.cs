namespace MiaCrate.Client.Models;

public class CubeDeformation
{
    public static readonly CubeDeformation None = new(0);
    
    private readonly float _growX;
    private readonly float _growY;
    private readonly float _growZ;

    public CubeDeformation(float growX, float growY, float growZ)
    {
        _growX = growX;
        _growY = growY;
        _growZ = growZ;
    }
    
    public CubeDeformation(float dilation)
        : this(dilation, dilation, dilation) {}

    public CubeDeformation Extend(float offset) => new(_growX + offset, _growY + offset, _growZ + offset);
    public CubeDeformation Extend(float offsetX, float offsetY, float offsetZ) =>
        new(_growX + offsetX, _growY + offsetY, _growZ + offsetZ);
}