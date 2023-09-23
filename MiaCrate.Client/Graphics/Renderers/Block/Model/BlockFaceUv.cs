using MiaCrate.Client.Models.Json;

namespace MiaCrate.Client.Graphics;

public class BlockFaceUv
{
    public float[]? Uvs { get; set; }
    public int Rotation { get; }

    public BlockFaceUv(float[]? uvs, int rotation)
    {
        Uvs = uvs;
        Rotation = rotation;
    }

    internal BlockFaceUv(JsonBlockElementFace payload)
        : this(ValidateUv(payload.Uv), ValidateRotation(payload.Rotation))
    {
    }
    
    public float GetU(int i)
    {
        var j = GetShiftedIndex(i);
        return Uvs![j != 0 && j != 1 ? 2 : 0];
    }

    public float GetV(int i)
    {
        var j = GetShiftedIndex(i);
        return Uvs![j != 0 && j != 3 ? 3 : 1];
    }

    public int GetReverseIndex(int i) => (i + 4 - Rotation / 90) % 4;

    private int GetShiftedIndex(int i) => (i + Rotation / 90) % 4;

    public void SetMissingUv(float[] uv) => Uvs ??= uv;

    private static int ValidateRotation(int rotation)
    {
        if (rotation >= 0 && rotation % 90 == 0 && rotation / 90 <= 3)
            return rotation;

        throw new Exception($"Invalid rotation {rotation} found, only 0/90/180/270 allowed");
    }

    private static float[]? ValidateUv(List<float>? uv)
    {
        if (uv == null) return null;
        
        if (uv.Count != 4)
            throw new Exception($"Expected 4 uv values, found: {uv.Count}");

        return uv.ToArray();
    }
}