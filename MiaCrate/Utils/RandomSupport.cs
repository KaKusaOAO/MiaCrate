namespace MiaCrate;

public static class RandomSupport
{
    private static long _seedUniquifier = 8682522807148012L;

    public static long GenerateUniqueSeed() => 
        UpdateAndGet(ref _seedUniquifier, l => l * 1181783497276652981L) ^ Util.NanoTime;

    private static long UpdateAndGet(ref long position, Func<long, long> update)
    {
        var prev = Interlocked.Read(ref position);
        var next = 0L;
        var hasNext = false;
        
        while (true)
        {
            if (!hasNext)
                next = update(prev);

            if (Interlocked.CompareExchange(ref position, next, prev) == prev)
                return next;

            hasNext = prev == (prev = Interlocked.Read(ref position));
        }
    }
}