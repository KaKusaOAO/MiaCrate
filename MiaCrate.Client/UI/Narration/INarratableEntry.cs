namespace MiaCrate.Client.UI.Narration;

public interface INarratableEntry : ITabOrderedElement, INarrationSupplier
{
    public NarrationPriority NarrationPriority { get; }
    public bool IsActive => true;
}