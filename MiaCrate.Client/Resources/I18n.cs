using System.Text.RegularExpressions;
using MiaCrate.Localizations;
using Mochi.Texts;

namespace MiaCrate.Client.Resources;

// ReSharper disable once InconsistentNaming
public static class I18n
{
    private static Language _language = Language.Instance;

    internal static void SetLanguage(Language language) => _language = language;

    public static string Get(string key, params object[] parameters)
    {
        var fmt = _language.GetOrDefault(key);

        try
        {
            var offset = 0;
            var counter = 0;
            var matches = new Regex("%(?:(?:(\\d*?)\\$)?)s").Matches(fmt);

            var result = new List<string>();
            foreach (Match m in matches)
            {
                var c = m.Groups[1].Value;
                var ci = c.Length == 0 ? counter++ : int.Parse(c) - 1;

                var front = fmt[offset..m.Index];
                if (front.Length > 0)
                    result.Add(front);

                result.Add(parameters[ci]?.ToString() ?? "<null>");
                offset = m.Index + m.Length;
            }

            result.Add(fmt[offset..]);
            return string.Join("", result);
        }
        catch (Exception)
        {
            return $"Format error: {fmt}";
        }
    }

    public static bool Exists(string key) => _language.Has(key);
}