namespace MiaCrate;

public interface IDependencySorterEntry<out T>
{
    public void VisitRequiredDependencies(Action<T> consumer);
    public void VisitOptionalDependencies(Action<T> consumer);
}