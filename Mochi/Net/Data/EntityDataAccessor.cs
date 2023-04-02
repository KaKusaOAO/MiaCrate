using Mochi.World.Entities;

namespace Mochi.Net.Data;

public interface IEntityDataAccessor
{
    public int Id { get; }
    public IEntityDataSerializer Serializer { get; }
}

public interface IEntityDataAccessor<T> : IEntityDataAccessor
{
    public new IEntityDataSerializer<T> Serializer { get; }
    IEntityDataSerializer IEntityDataAccessor.Serializer => Serializer;
}

public class EntityDataAccessor<T> : IEntityDataAccessor<T>
{
    public int Id { get; }
    public IEntityDataSerializer<T> Serializer { get; }
    
    public EntityDataAccessor(int id, IEntityDataSerializer<T> serializer)
    {
        Id = id;
        Serializer = serializer;
    }
}