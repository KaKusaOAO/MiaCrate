namespace MiaCrate.Client;

public class ComponentCollector
{
    private readonly List<IFormattedText> _parts = new();

    public IFormattedText? Result
    {
        get
        {
            if (!_parts.Any()) return null;

            return _parts.Count == 1
                ? _parts.First()
                : IFormattedText.Composite(_parts);
        }
    }

    public IFormattedText ResultOrEmpty => Result ?? IFormattedText.Empty;

    public void Append(IFormattedText text) => _parts.Add(text);
    
    public void Reset() => _parts.Clear();
}