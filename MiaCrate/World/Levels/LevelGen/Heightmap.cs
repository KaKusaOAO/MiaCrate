using MiaCrate.World.Blocks;

namespace MiaCrate.World;

public class Heightmap
{
    private static Predicate<BlockState> NotAir { get; } = b => !b.IsAir;

    private static Predicate<BlockState> MaterialMotionBlocking { get; } = _ => throw new NotImplementedException();

    public enum Usage
    {
        WorldGen, 
        LiveWorld,
        Client
    }

    [Flags]
    public enum Types
    {
        // @formatter:off
        WorldSurfaceWG         = 1 << 0,
        WorldSurface           = 1 << 1,
        OceanFloorWG           = 1 << 2,
        OceanFloor             = 1 << 3,
        MotionBlocking         = 1 << 4,
        MotionBlockingNoLeaves = 1 << 5
        // @formatter:on
    }
    
    public sealed class TypeDescription : IStringRepresentable
    {
        private static readonly Dictionary<Types, TypeDescription> _values = new();

        public static TypeDescription WorldSurfaceWG { get; } = Register(Types.WorldSurfaceWG, 
        new TypeDescription("WORLD_SURFACE_WG", Usage.WorldGen, NotAir));
        
        public static TypeDescription WorldSurface { get; } = Register(Types.WorldSurface, 
        new TypeDescription("WORLD_SURFACE", Usage.Client, NotAir));
        
        public static TypeDescription OceanFloorWG { get; } = Register(Types.OceanFloorWG, 
        new TypeDescription("OCEAN_FLOOR_WG", Usage.WorldGen, MaterialMotionBlocking));
        
        public static TypeDescription OceanFloor { get; } = Register(Types.OceanFloor, 
        new TypeDescription("OCEAN_FLOOR", Usage.LiveWorld, MaterialMotionBlocking));
        
        public static TypeDescription MotionBlocking { get; } = Register(Types.MotionBlocking, 
        new TypeDescription("MOTION_BLOCKING", Usage.Client, _ => throw new NotImplementedException()));
        
        public static TypeDescription MotionBlockingNoLeaves { get; } = Register(Types.MotionBlockingNoLeaves, 
            new TypeDescription("MOTION_BLOCKING_NO_LEAVES", Usage.LiveWorld, _ => throw new NotImplementedException()));
        
        private readonly Usage _usage;
        
        public Predicate<BlockState> IsOpaque { get; }
        public string SerializationKey { get; }
        public string SerializedName => SerializationKey;

        private TypeDescription(string serializationKey, Usage usage, Predicate<BlockState> isOpaque)
        {
            SerializationKey = serializationKey;
            _usage = usage;
            IsOpaque = isOpaque;
        }

        private static TypeDescription Register(Types type, TypeDescription desc)
        {
            if (_values.ContainsKey(type))
                throw new ArgumentException($"Duplicated type description for {type}");
            
            _values.Add(type, desc);
            return desc;
        }
    } 
}

public static class HeightmapTypesExtension
{
    public static List<Heightmap.TypeDescription> GetDescriptions(this Heightmap.Types type)
    {
        var list = new List<Heightmap.TypeDescription>();
        
        if (type.HasFlag(Heightmap.Types.WorldSurfaceWG))
            list.Add(Heightmap.TypeDescription.WorldSurfaceWG);
        
        if (type.HasFlag(Heightmap.Types.WorldSurface))
            list.Add(Heightmap.TypeDescription.WorldSurface);
        
        if (type.HasFlag(Heightmap.Types.OceanFloorWG))
            list.Add(Heightmap.TypeDescription.OceanFloorWG);
        
        if (type.HasFlag(Heightmap.Types.OceanFloor))
            list.Add(Heightmap.TypeDescription.OceanFloor);
        
        if (type.HasFlag(Heightmap.Types.MotionBlocking))
            list.Add(Heightmap.TypeDescription.MotionBlocking);
        
        if (type.HasFlag(Heightmap.Types.MotionBlockingNoLeaves))
            list.Add(Heightmap.TypeDescription.MotionBlockingNoLeaves);

        return list;
    }
    
    public static Heightmap.Types EnsureSingle(this Heightmap.Types type)
    {
        if (!Enum.GetValues<Heightmap.Types>().Contains(type))
            throw new InvalidOperationException("Cannot have multiple heightmap types in this context");

        return type;
    }
}