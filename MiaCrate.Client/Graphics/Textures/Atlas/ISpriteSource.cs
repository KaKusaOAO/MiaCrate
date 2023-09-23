using MiaCrate.Resources;

namespace MiaCrate.Client.Graphics;

public interface ISpriteSource
{
    public static readonly FileToIdConverter TextureIdConverter = new("textures", ".png");

    public SpriteSourceType Type { get; }
    
    public void Run(IResourceManager manager, IOutput output);

    public interface ISpriteSupplier
    {
        public SpriteContents? Apply(ISpriteResourceLoader loader);
        
        public void Discard() {}

        public static ISpriteSupplier Create(Func<ISpriteResourceLoader, SpriteContents?> func) =>
            new Instance(func);

        private class Instance : ISpriteSupplier
        {
            private readonly Func<ISpriteResourceLoader, SpriteContents?> _func;

            public Instance(Func<ISpriteResourceLoader, SpriteContents?> func)
            {
                _func = func;
            }

            public SpriteContents? Apply(ISpriteResourceLoader loader) => _func(loader);
        }
    }
    
    public interface IOutput
    {
        public void Add(ResourceLocation location, ISpriteSupplier supplier);
        public void RemoveAll(Predicate<ResourceLocation> predicate);

        public void Add(ResourceLocation location, Resource resource) => 
            Add(location, ISpriteSupplier.Create(loader => loader.LoadSprite(location, resource)));
    }
}