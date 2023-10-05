namespace MiaCrate.World;

public interface ILevelCallback<in T>
{
    public void OnTrackingStart(T obj);
}