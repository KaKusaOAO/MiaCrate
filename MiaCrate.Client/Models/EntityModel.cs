using MiaCrate.World.Entities;

namespace MiaCrate.Client.Models;

public interface IEntityModel : IModel
{
    
}

public interface IEntityModel<T> : IEntityModel where T : Entity
{
    
}

public abstract class EntityModel<T> : Model, IEntityModel<T> where T : Entity
{
    
}