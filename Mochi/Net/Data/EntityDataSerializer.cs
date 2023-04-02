using KaLib.Utils;
using Mochi.Extensions;

namespace Mochi.Net.Data;

public static class EntityDataSerializer
{
    public abstract class ForValueType<T> : IEntityDataSerializer<T>
    {
        public abstract void Write(Stream stream, T val);
        public abstract T Read(Stream stream);
        public T Copy(T val) => val;
    }

    private class Simple<T> : ForValueType<T>
    {
        private readonly Action<Stream, T> _write;
        private readonly Func<Stream, T> _read;

        public Simple(Action<Stream, T> write, Func<Stream, T> read)
        {
            _write = write;
            _read = read;
        }

        public override void Write(Stream stream, T val) => _write(stream, val);

        public override T Read(Stream stream) => _read(stream);
    }

    public static IEntityDataAccessor<T> CreateAccessor<T>(this IEntityDataSerializer<T> serializer, int i) => 
        new EntityDataAccessor<T>(i, serializer);

    public static IEntityDataSerializer<T> MakeSimple<T>(Action<Stream, T> write, Func<Stream, T> read) =>
        new Simple<T>(write, read);

    public static IEntityDataSerializer<IOptional<T>> MakeOptional<T>(Action<Stream, T> write, Func<Stream, T> read) 
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
    
    public static IEntityDataSerializer<IOptional<T>> MakeOptional<T>(Action<Stream, T> write, Func<Stream, T?> read) 
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