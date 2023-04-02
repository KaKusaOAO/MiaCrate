using Mochi.IO;
using Mochi.Utils;

namespace MiaCrate.Net.Data;

public static class EntityDataSerializer
{
    public abstract class ForValueType<T> : IEntityDataSerializer<T>
    {
        public abstract void Write(BufferWriter stream, T val);
        public abstract T Read(BufferReader stream);
        public T Copy(T val) => val;
    }

    private class Simple<T> : ForValueType<T>
    {
        private readonly Action<BufferWriter, T> _write;
        private readonly Func<BufferReader, T> _read;

        public Simple(Action<BufferWriter, T> write, Func<BufferReader, T> read)
        {
            _write = write;
            _read = read;
        }

        public override void Write(BufferWriter stream, T val) => _write(stream, val);

        public override T Read(BufferReader stream) => _read(stream);
    }

    public static IEntityDataAccessor<T> CreateAccessor<T>(this IEntityDataSerializer<T> serializer, int i) => 
        new EntityDataAccessor<T>(i, serializer);

    public static IEntityDataSerializer<T> MakeSimple<T>(Action<BufferWriter, T> write, Func<BufferReader, T> read) =>
        new Simple<T>(write, read);

    public static IEntityDataSerializer<IOptional<T>> MakeOptional<T>(Action<BufferWriter, T> write, Func<BufferReader, T> read) 
        where T : class
    {
        return new Simple<IOptional<T>>((stream, opt) =>
        {
            if (opt.IsPresent)
            {
                stream.WriteBool(true);
                write(stream, opt.Value);
            }
            else
            {
                stream.WriteBool(false);
            }
        }, s => Optional.OfNullable(read(s)));
    }
    
    public static IEntityDataSerializer<IOptional<T>> MakeOptional<T>(Action<BufferWriter, T> write, Func<BufferReader, T?> read) 
        where T : struct
    {
        return new Simple<IOptional<T>>((stream, opt) =>
        {
            if (opt.IsPresent)
            {
                stream.WriteBool(true);
                write(stream, opt.Value);
            }
            else
            {
                stream.WriteBool(false);
            }
        }, s => Optional.OfNullable(read(s)));
    }
}