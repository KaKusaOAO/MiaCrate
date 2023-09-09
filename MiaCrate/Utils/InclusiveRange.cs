using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate;

public static class InclusiveRange
{
    public static readonly ICodec<InclusiveRange<int>> Int = Codec(Data.Codec.Int);
    
    public static ICodec<InclusiveRange<T>> Codec<T>(ICodec<T> codec) where T : IComparable<T>
    {
        return ExtraCodecs.IntervalCodec(codec, 
            "min_inclusive", "max_inclusive", Create, 
            x => x.MinInclusive, 
            x => x.MaxInclusive);
    }

    public static IDataResult<InclusiveRange<T>> Create<T>(T minInclusive, T maxInclusive) where T : IComparable<T>
    {
        return minInclusive.CompareTo(maxInclusive) <= 0
            ? DataResult.Success(new InclusiveRange<T>(minInclusive, maxInclusive))
            : DataResult.Error<InclusiveRange<T>>(() => "min_inclusive must be less than or equal to max_inclusive");
    }
}

public class InclusiveRange<T> where T : IComparable<T>
{
    public InclusiveRange(T minInclusive, T maxInclusive)
    {
        if (minInclusive.CompareTo(maxInclusive) > 0)
            throw new ArgumentException("min_inclusive must be less than or equal to max_inclusive");
        
        MinInclusive = minInclusive;
        MaxInclusive = maxInclusive;
    }
    
    public InclusiveRange(T single) : this(single, single) {}

    public T MinInclusive { get; }
    public T MaxInclusive { get; }

    public void Deconstruct(out T minInclusive, out T maxInclusive)
    {
        minInclusive = MinInclusive;
        maxInclusive = MaxInclusive;
    }
}