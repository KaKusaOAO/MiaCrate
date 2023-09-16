namespace MiaCrate.Json;

public class ChainedJsonException : IOException
{
    private readonly List<Entry> _entries = new();
    private readonly string _message;

    public override string Message => $"Invalid {_entries.Last()}: {_message}";

    public ChainedJsonException(string message)
    {
        _entries.Add(new Entry());
        _message = message;
    }

    public ChainedJsonException(string message, Exception inner) : base(null, inner)
    {
        _entries.Add(new Entry());
        _message = message;
    }

    public void PrependJsonKey(string key) => _entries.First().AddJsonKey(key);

    public void SetFileNameAndFlush(string fileName)
    {
        _entries.First().FileName = fileName;
        _entries.Insert(0, new Entry());
    }

    public static ChainedJsonException ForException(Exception ex)
    {
        if (ex is ChainedJsonException chained) return chained;

        var message = ex.Message;
        if (ex is FileNotFoundException)
        {
            message = "File not found";
        }

        return new ChainedJsonException(message, ex);
    }

    public class Entry
    {
        public string? FileName { get; internal set; }
        private readonly List<string> _jsonKeys = new();

        public void AddJsonKey(string key) => _jsonKeys.Insert(0, key);
        public string JsonKeys => string.Join("->", _jsonKeys);

        public override string ToString()
        {
            var fileName = FileName ?? "(Unknown file)";
            return $"{fileName} {JsonKeys}".Trim();
        }
    }
}