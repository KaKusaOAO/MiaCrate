using System.Numerics;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IDynamicOps
{
    object? Empty { get; }
    object? EmptyMap { get; }
    object? EmptyList { get; }
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

    IDynamicOps<TOut> ConvertTo<TOut>(IDynamicOps<TOut> outOps, T input);
    new T CreateString(string value);
    object? IDynamicOps.CreateString(string value) => CreateString(value);

    IDataResult<string> GetStringValue(T value);
    IDataResult<string> IDynamicOps.GetStringValue(object? value) => GetStringValue((T)value!);

    IDataResult<decimal> GetNumberValue(T value);
    IDataResult<decimal> IDynamicOps.GetNumberValue(object? value) => GetNumberValue((T)value!);

    new IRecordBuilder<T> MapBuilder { get; }
    IRecordBuilder IDynamicOps.MapBuilder => MapBuilder;

    IDataResult<IEnumerable<T>> GetEnumerable(T input);
    T CreateList(IEnumerable<T> input);

    IDataResult<T> MergeToMap(T prefix, IDictionary<T, T> map) =>
        MergeToMap(prefix, MapLike.ForDictionary(map, this));
    IDataResult<T> MergeToMap(T prefix, IMapLike<T> mapLike);

    IDataResult<T> MergeToList(T list, T value);

    IDataResult<T> MergeToList(T list, IEnumerable<T> values)
    {
        var result = DataResult.Success(list);
        return values.Aggregate(result, (current, value) => current.SelectMany(r => MergeToList(r, value)));
    }
}