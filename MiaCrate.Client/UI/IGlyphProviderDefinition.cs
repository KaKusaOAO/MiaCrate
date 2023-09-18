using MiaCrate.Client.Fonts;
using MiaCrate.Data;
using MiaCrate.Resources;

namespace MiaCrate.Client.UI;

public interface IGlyphProviderDefinition
{
    public GlyphProviderType Type { get; }
    public IEither<ILoader, Reference> Unpack();

    public record Reference(ResourceLocation Id);
    
    public interface ILoader
    {
        public IGlyphProvider Load(IResourceManager manager);

        public static ILoader Create(Func<IResourceManager, IGlyphProvider> func) => new Instance(func);

        private class Instance : ILoader
        {
            private readonly Func<IResourceManager, IGlyphProvider> _func;

            public Instance(Func<IResourceManager, IGlyphProvider> func)
            {
                _func = func;
            }

            public IGlyphProvider Load(IResourceManager manager) => _func(manager);
        }
    }
}