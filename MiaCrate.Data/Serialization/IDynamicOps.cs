using System.Numerics;
using MiaCrate.Data.Codecs;
using Mochi.Utils;

namespace MiaCrate.Data;

public interface IDynamicOps
{
    IDynamic CreateEmptyDynamic();
    
    object? Empty { get; }
    object? EmptyMap => CreateMap(Enumerable.Empty<IPair>());
    object? EmptyList => CreateList(Enumerable.Empty<object>());
    public object CreateList(IEnumerable<object> input);

    public object CreateMap(IEnumerable<IPair> map);
    object? CreateString(string value);
    IDataResult<string> GetStringValue(object? value);
    IDataResult<decimal> GetNumberValue(object? value);
    IRecordBuilder MapBuilder { get; }
    bool CompressMaps => false;
}

public interface IDynamicOps<T> : IDynamicOps
{
    new T Empty { get; }
    object? IDynamicOps.Empty => Empty;

    new T EmptyMap => CreateMap(Enumerable.Empty<IPair<T, T>>());
    object? IDynamicOps.EmptyMap => EmptyMap;

    new T EmptyList => CreateList(Enumerable.Empty<T>());
    object? IDynamicOps.EmptyList => EmptyList;

    new IDynamic<T> CreateEmptyDynamic() => new Dynamic<T>(this);
    IDynamic IDynamicOps.CreateEmptyDynamic() => CreateEmptyDynamic();

    TOut ConvertTo<TOut>(IDynamicOps<TOut> outOps, T input);
    new T CreateString(string value);
    object? IDynamicOps.CreateString(string value) => CreateString(value);

    public T CreateNumeric(decimal val);

    public T CreateByte(byte val) => CreateNumeric(val);
    public T CreateShort(short val) => CreateNumeric(val);
    public T CreateInt(int val) => CreateNumeric(val);
    public T CreateLong(long val) => CreateNumeric(val);
    public T CreateFloat(float val) => CreateNumeric((decimal) val);
    public T CreateDouble(double val) => CreateNumeric((decimal) val);
    public T CreateBool(bool val) => CreateByte((byte) (val ? 1 : 0));

    IDataResult<string> GetStringValue(T value);
    IDataResult<string> IDynamicOps.GetStringValue(object? value) => GetStringValue((T)value!);

    IDataResult<decimal> GetNumberValue(T value);
    IDataResult<decimal> IDynamicOps.GetNumberValue(object? value) => GetNumberValue((T)value!);

    IDataResult<bool> GetBoolValue(T input) =>
        GetNumberValue(input).Select(n => n != 0);

    new IRecordBuilder<T> MapBuilder { get; }
    IRecordBuilder IDynamicOps.MapBuilder => MapBuilder;

    public IDataResult<IEnumerable<T>> GetEnumerable(T input);
    public IDataResult<IEnumerable<IPair<T, T>>> GetMapValues(T input);

    public IDataResult<Action<Action<T, T>>> GetMapEntries(T input)
    {
        return GetMapValues(input).Select<Action<Action<T, T>>>(s => c =>
        {
            foreach (var p in s)
            {
                c(p.First, p.Second);
            }
        });
    }
    
    public T CreateList(IEnumerable<T> input);
    object IDynamicOps.CreateList(IEnumerable<object> input) => CreateList(input.Cast<T>())!;

    public T CreateMap(IEnumerable<IPair<T, T>> map);
    object IDynamicOps.CreateMap(IEnumerable<IPair> input) => CreateMap(input.Cast<IPair<T, T>>())!;
    
    IDataResult<T> MergeToMap(T prefix, IDictionary<T, T> map) =>
        MergeToMap(prefix, MapLike.ForDictionary(map, this));
    IDataResult<T> MergeToMap(T prefix, IMapLike<T> mapLike);

    IDataResult<T> MergeToList(T list, T value);

    IDataResult<T> MergeToList(T list, IEnumerable<T> values)
    {
        var result = DataResult.Success(list);
        return values.Aggregate(result, (current, value) => current.SelectMany(r => MergeToList(r, value)));
    }

    IDataResult<T> MergeToPrimitive(T prefix, T value) => !Equals(prefix, Empty)
        ? DataResult.Error<T>(() => $"Do not know how to append a primitive value {value} to {prefix}")
        : DataResult.Success(value);

    public TOut ConvertList<TOut>(IDynamicOps<TOut> outOps, T input) =>
        outOps.CreateList(GetEnumerable(input).Result.OrElse(Enumerable.Empty<T>())
            .Select(e => ConvertTo(outOps, e)));

    public TOut ConvertMap<TOut>(IDynamicOps<TOut> outOps, T input) =>
        outOps.CreateMap(GetMapValues(input).Result.OrElse(Enumerable.Empty<IPair<T, T>>())
            .Select(e => Pair.Of(ConvertTo(outOps, e.First), ConvertTo(outOps, e.Second))));
}