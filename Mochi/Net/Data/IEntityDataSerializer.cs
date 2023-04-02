using Mochi.World.Entities;

namespace Mochi.Net.Data;

public interface IEntityDataSerializer
{
    public void Write(Stream stream, object val);
    public object Read(Stream stream);
    public object Copy(object val);
}

public interface IEntityDataSerializer<T> : IEntityDataSerializer
{
    public void Write(Stream stream, T val);
    void IEntityDataSerializer.Write(Stream stream, object val) => Write(stream, (T) val);
    
    public new T Read(Stream stream);
    object IEntityDataSerializer.Read(Stream stream) => Read(stream)!;

    public T Copy(T val);
    object IEntityDataSerializer.Copy(object val) => Copy((T) val)!;
}