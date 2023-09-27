using MiaCrate.Localizations;
using MiaCrate.Resources;
using MiaCrate.Texts;
using Mochi.Core;
using Mochi.Utils;

namespace MiaCrate.Client.Resources;

public class ClientLanguage : Language
{
    private readonly Dictionary<string, string> _storage;
    public override bool IsDefaultRightToLeft { get; }
    
    private ClientLanguage(Dictionary<string, string> storage, bool rtl)
    {
        _storage = storage;
        IsDefaultRightToLeft = rtl;
    }

    public static ClientLanguage LoadFrom(IResourceManager manager, List<string> list, bool rtl)
    {
        var dict = new Dictionary<string, string>();
        
        foreach (var locale in list)
        {
            var path = $"lang/{locale}.json";

            foreach (var ns in manager.Namespaces)
            {
                try
                {
                    var location = new ResourceLocation(ns, path);
                    AppendFrom(locale, manager.GetResourceStack(location), dict);
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Skipped language file: {ns}:{path} ({ex})");
                }
            }
        }

        return new ClientLanguage(dict, rtl);
    }

    private static void AppendFrom(string locale, List<Resource> resources, Dictionary<string, string> map)
    {
        foreach (var resource in resources)
        {
            try
            {
                using var stream = resource.Open();
                LoadFromJson(stream, (k, v) => map[k] = v);
            }
            catch (IOException ex)
            {
                Logger.Warn($"Failed to load translations for {locale} from pack {resource.Source.PackId}");
                Logger.Warn(ex);
            }
        }
    }

    public override string GetOrDefault(string key, string defaultValue)
    {
        throw new NotImplementedException();
    }

    public override bool Has(string key)
    {
        throw new NotImplementedException();
    }

    public override FormattedCharSequence GetVisualOrder(IFormattedText text)
    {
        // TODO: Bidi support
        return sink => text.Visit((style, str) =>
            StringDecomposer.IterateFormatted(str, style, sink)
                ? Optional.Empty<Unit>()
                : IFormattedText.StopIteration, Style.Empty).IsPresent;
    }
}