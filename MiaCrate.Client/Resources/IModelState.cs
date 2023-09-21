namespace MiaCrate.Client.Resources;

public interface IModelState
{
    public Transformation Rotation => Transformation.Identity;
    public bool IsUvLocked => false;
}