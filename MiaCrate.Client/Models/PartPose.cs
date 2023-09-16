namespace MiaCrate.Client.Models;

public class PartPose
{
    public static readonly PartPose Zero = OffsetAndRotation(0, 0, 0, 0, 0, 0);
    
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public float XRot { get; }
    public float YRot { get; }
    public float ZRot { get; }

    private PartPose(float x, float y, float z, float xRot, float yRot, float zRot)
    {
        X = x;
        Y = y;
        Z = z;
        XRot = xRot;
        YRot = yRot;
        ZRot = zRot;
    }
    
    public static PartPose Offset(float x, float y, float z) =>
        OffsetAndRotation(x, y, z, 0, 0, 0);

    public static PartPose Rotation(float xRot, float yRot, float zRot) =>
        OffsetAndRotation(0, 0, 0, xRot, yRot, zRot);
    
    public static PartPose OffsetAndRotation(float x, float y, float z, float xRot, float yRot, float zRot) =>
        new(x, y, z, xRot, yRot, zRot);
}