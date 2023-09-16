namespace MiaCrate.Client;

public abstract class ByteBuffer
{
    public abstract int Position { get; protected set; }
    public abstract int Limit { get; protected set; }
    public abstract int Capacity { get; }

    public void SetPosition(int pos)
    {
        if (pos > Limit || pos < 0)
            throw new ArgumentOutOfRangeException(nameof(pos));

        Position = pos;
    }

    public void SetLimit(int limit)
    {
        if (limit > Capacity || limit < 0)
            throw new ArgumentOutOfRangeException(nameof(limit));

        if (Position > limit) Position = limit;
        Limit = limit;
    }

    public void Flip()
    {
        Limit = Position;
        Position = 0;
    }

    public void Clear()
    {
        Position = 0;
        Limit = Capacity;
    }

    public void Rewind()
    {
        Position = 0;
    }

    public abstract void Put(int index, byte[] arr);
    public abstract void Put(byte[] arr);

    public abstract void Resize(int capacity);
}