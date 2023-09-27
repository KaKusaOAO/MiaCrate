using MiaCrate.Core;
using MiaCrate.Data;

namespace MiaCrate;

public interface IIntProviderType
{
    public ICodec<IntProvider> Codec { get; }

    public static IIntProviderType<ConstantInt> Constant { get; } = Register("constant", ConstantInt.Codec);

    public static IIntProviderType<T> Create<T>(ICodec<T> codec) where T : IntProvider => new InstanceDirect<T>(codec);
    public static IIntProviderType<T> Create<T>(Func<ICodec<T>> codec) where T : IntProvider => new Instance<T>(codec);

    public static IIntProviderType<T> Register<T>(string name, ICodec<T> codec) where T : IntProvider =>
        (IIntProviderType<T>) Registry.Register(BuiltinRegistries.IntProviderType, name, Create(codec));

    private class Instance<T> : IIntProviderType<T> where T : IntProvider
    {
        private readonly Func<ICodec<T>> _func;

        public ICodec<T> Codec => _func();

        public Instance(Func<ICodec<T>> func)
        {
            _func = func;
        }
    }
    
    private class InstanceDirect<T> : IIntProviderType<T> where T : IntProvider
    {
        public ICodec<T> Codec { get; }

        public InstanceDirect(ICodec<T> codec)
        {
            Codec = codec;
        }
    }
}

public interface IIntProviderType<T> : IIntProviderType where T : IntProvider
{
    public new ICodec<T> Codec { get; }
    ICodec<IntProvider> IIntProviderType.Codec => Codec.Cast<IntProvider>();
}