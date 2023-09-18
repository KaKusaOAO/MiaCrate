using OpenTK.Mathematics;

namespace MiaCrate.Client.Extensions;

public static class DataExtension
{
    public static Vector3 ToVector3(this List<float> source)
    {
        if (source.Count != 3)
            throw new ArgumentException("List doesn't have exactly 3 elements");

        return new Vector3(source[0], source[1], source[2]);
    }
}