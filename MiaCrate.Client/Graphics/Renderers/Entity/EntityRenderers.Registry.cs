using MiaCrate.World.Entities;

namespace MiaCrate.Client.Graphics;

public static partial class EntityRenderers
{
    static EntityRenderers()
    {
        Register<Allay, AllayRenderer>(EntityType.Allay);
    }
}