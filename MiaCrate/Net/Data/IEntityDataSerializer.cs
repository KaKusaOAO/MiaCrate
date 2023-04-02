using MiaCrate.World.Entities;
using Mochi.IO;

namespace MiaCrate.Net.Data;

public interface IEntityDataSerializer
{
    public void Write(BufferWriter stream, object val);
    public object Read(BufferReader stream);
    public object Copy(object val);
}

public interface IEntityDataSerializer<T> : IEntityDataSerializer
{
    public void Write(BufferWriter stream, T val);
    void IEntityDataSerializer.Write(BufferWriter stream, object val) => Write(stream, (T) val);
    
    public new T Read(BufferReader stream);
    object IEntityDataSerializer.Read(BufferReader stream) => Read(stream)!;

    public T Copy(T val);
    object IEntityDataSerializer.Copy(object val) => Copy((T) val)!;
}