using MiaCrate.Core;

namespace MiaCrate.Client.Graphics;

public sealed class FaceInfo
{
    public static readonly FaceInfo Down = new(
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MaxZ),
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MaxZ)
    );
    
    public static readonly FaceInfo Up = new(
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MinZ),
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MinZ)
    );
    
    public static readonly FaceInfo North = new(
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MinZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MinZ)
    );
    
    public static readonly FaceInfo South = new(
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MaxZ),
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MaxZ)
    );
    
    public static readonly FaceInfo West = new(
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MinZ),
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MinX, Constants.MinY, Constants.MaxZ),
        new VertexInfo(Constants.MinX, Constants.MaxY, Constants.MaxZ)
    );
    
    public static readonly FaceInfo East = new(
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MaxZ),
        new VertexInfo(Constants.MaxX, Constants.MinY, Constants.MinZ),
        new VertexInfo(Constants.MaxX, Constants.MaxY, Constants.MinZ)
    );
    
    private static readonly FaceInfo[] _byFaces = Util.Make(new FaceInfo[6], arr =>
    {
        arr[Constants.MinY] = Down;
        arr[Constants.MaxY] = Up;
        arr[Constants.MinZ] = North;
        arr[Constants.MaxZ] = South;
        arr[Constants.MinX] = West;
        arr[Constants.MaxX] = East;
    });
    
    private readonly VertexInfo[] _infos;

    private FaceInfo(params VertexInfo[] infos)
    {
        _infos = infos;
    }

    public static FaceInfo FromFacing(Direction direction) => _byFaces[direction.Data3d];

    public VertexInfo GetVertexInfo(int index) => _infos[index];

    public record VertexInfo(int XFace, int YFace, int ZFace);
    
    public static class Constants
    {
        public static int MaxZ { get; } = Direction.South.Data3d;
        public static int MaxY { get; } = Direction.Up.Data3d;
        public static int MaxX { get; } = Direction.East.Data3d;
        public static int MinZ { get; } = Direction.North.Data3d;
        public static int MinY { get; } = Direction.Down.Data3d;
        public static int MinX { get; } = Direction.West.Data3d;
    }
}