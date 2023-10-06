using MiaCrate.Core;
using MiaCrate.Data;
using MiaCrate.Data.Codecs;

namespace MiaCrate.Resources;

public static class RegistryFileCodec
{
    public static RegistryFileCodec<T> Create<T>(IResourceKey<IRegistry<T>> key, ICodec<T> codec, bool bl = true)
        where T : class => new(key, codec, bl);
}

public class RegistryFileCodec<T> : ICodec<IHolder<T>> where T : class
{
    private readonly IResourceKey<IRegistry<T>> _registryKey;
    private readonly ICodec<T> _elementCodec;
    private readonly bool _allowInline;

    public RegistryFileCodec(IResourceKey<IRegistry<T>> registryKey, ICodec<T> elementCodec, bool allowInline)
    {
        _registryKey = registryKey;
        _elementCodec = elementCodec;
        _allowInline = allowInline;
    }

    public IDataResult<TDynamic> Encode<TDynamic>(IHolder<T> input, IDynamicOps<TDynamic> ops, TDynamic prefix)
    {
        throw new NotImplementedException();
    }

    public IDataResult<IPair<IHolder<T>, TIn>> Decode<TIn>(IDynamicOps<TIn> ops, TIn input)
    {
        throw new NotImplementedException();
    }
}