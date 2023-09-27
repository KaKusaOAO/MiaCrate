using MiaCrate.Core;
using MiaCrate.Data;

namespace MiaCrate;

public interface IFloatProviderType
{
    public ICodec<FloatProvider> Codec { get; }

    public static IFloatProviderType<ConstantFloat> Constant { get; } = Register("constant", ConstantFloat.Codec);

    public static IFloatProviderType<T> Create<T>(ICodec<T> codec) where T : FloatProvider => new InstanceDirect<T>(codec);
    public static IFloatProviderType<T> Create<T>(Func<ICodec<T>> codec) where T : FloatProvider => new Instance<T>(codec);

    public static IFloatProviderType<T> Register<T>(string name, ICodec<T> codec) where T : FloatProvider =>
        (IFloatProviderType<T>) Registry.Register(BuiltinRegistries.FloatProviderType, name, Create(codec));

    private class Instance<T> : IFloatProviderType<T> where T : FloatProvider
    {
        private readonly Func<ICodec<T>> _func;

        public ICodec<T> Codec => _func();

        public Instance(Func<ICodec<T>> func)
        {
            _func = func;
        }
    }
    
    private class InstanceDirect<T> : IFloatProviderType<T> where T : FloatProvider
    {
        public ICodec<T> Codec { get; }

        public InstanceDirect(ICodec<T> codec)
        {
            Codec = codec;
        }
    }
}

public interface IFloatProviderType<T> : IFloatProviderType where T : FloatProvider
{
    public new ICodec<T> Codec { get; }
    ICodec<FloatProvider> IFloatProviderType.Codec => Codec.Cast<FloatProvider>();
}