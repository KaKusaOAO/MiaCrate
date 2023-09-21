using System.Diagnostics.CodeAnalysis;
using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public static partial class EntityRenderers
{
    private static readonly Dictionary<IEntityType, IEntityRendererProvider> _providers = new();

    private static void Register<T>(IEntityType<T> type, EntityRendererProvider<T> provider) where T : Entity
    {
        _providers[type] = IEntityRendererProvider.Of(provider);
    }

    private static void Register
        <TEntity, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TRenderer>(
        IEntityType<TEntity> type) 
        where TEntity : Entity where TRenderer : EntityRenderer<TEntity>
    {
        var ctor = typeof(TRenderer).GetConstructor(new[] {typeof(EntityRendererContext)});
        if (ctor == null)
            throw new InvalidOperationException($"Type {typeof(TRenderer)} doesn't contain a standard constructor of an entity renderer");

        Register(type, ctx => (TRenderer) ctor.Invoke(new object[] { ctx }));
    }
}