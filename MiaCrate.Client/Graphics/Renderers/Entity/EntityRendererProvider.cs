using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public delegate EntityRenderer<T> EntityRendererProvider<T>(EntityRendererContext context) where T : Entity;

public interface IEntityRendererProvider
{
    IEntityRenderer Create(EntityRendererContext context);

    public static IEntityRendererProvider Of<T>(EntityRendererProvider<T> provider) where T : Entity =>
        new Instance<T>(provider);

    private class Instance<T> : IEntityRendererProvider where T : Entity
    {
        private readonly EntityRendererProvider<T> _provider;

        public Instance(EntityRendererProvider<T> provider)
        {
            _provider = provider;
        }

        public IEntityRenderer Create(EntityRendererContext context) => _provider(context);
    }
}