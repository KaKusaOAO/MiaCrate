namespace MiaCrate.Client.UI;

public interface INarratableEntry
{
    public NarrationPriority NarrationPriority { get; }
    public bool IsActive => true;
}