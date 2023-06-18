using MiaCrate.Data.Codecs;

namespace MiaCrate.Data;

public interface IDynamicOps
{
    object? Empty { get; }
    object? EmptyMap { get; }
    object? EmptyList { get; }
    object? CreateString(string value);
    IDataResult<string> GetStringValue(object? value);
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
}