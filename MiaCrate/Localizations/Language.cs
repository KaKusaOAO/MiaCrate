using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MiaCrate.Resources;
using Mochi.Utils;

namespace MiaCrate.Localizations;

public abstract class Language
{
    public const string Default = "en_us";
    
    private static readonly Regex _unsupportedFormatRegex = new("%(\\d+\\$)?[\\d.]*[df]");
    
    public static Language Instance { get; private set; } = LoadDefault();

    public static void Inject(Language language) => Instance = language;

    public string GetOrDefault(string key) => GetOrDefault(key, key);

    public abstract string GetOrDefault(string key, string defaultValue);

    public abstract bool Has(string key);

    public abstract bool IsDefaultRightToLeft { get; }

    private static Language LoadDefault()
    {
        var dict = new Dictionary<string, string>();
        ParseTranslations((k, v) => dict[k] = v, $"assets/minecraft/lang/{Default}.json");
        return new DefaultLanguage(dict);
    }

    private static void ParseTranslations(Action<string, string> consumer, string path)
    {
        try
        {
            var archive = ResourceAssembly.GameArchive;
            using var stream = archive.GetEntry(path)!.Open();
            LoadFromJson(stream, consumer);
        }
        catch (Exception ex)
        {
            void Handle()
            {
                Logger.Error($"Couldn't read strings from {path}");
                Logger.Error(ex);
            }

            if (ex is not (IOException or JsonException)) throw;
            Handle();
        }
    }

    public static void LoadFromJson(Stream stream, Action<string, string> consumer)
    {
        var obj = JsonSerializer.Deserialize<JsonObject>(stream)!;
        foreach (var (key, value) in obj)
        {
            var content = _unsupportedFormatRegex.Replace(value!.GetValue<string>(), "%$1s");
            consumer(key, content);
        }
    }

    private class DefaultLanguage : Language
    {
        private readonly Dictionary<string, string> _map;

        public DefaultLanguage(Dictionary<string, string> map)
        {
            _map = map;
        }

        public override string GetOrDefault(string key, string defaultValue) =>
            _map.GetValueOrDefault(key, defaultValue);

        public override bool Has(string key) => _map.ContainsKey(key);

        public override bool IsDefaultRightToLeft => false;
    }
}