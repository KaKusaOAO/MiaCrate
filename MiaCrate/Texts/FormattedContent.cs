using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MiaCrate.Localizations;
using Mochi.Texts;

namespace MiaCrate.Texts;

public partial class TranslatableContent : IContent<TranslatableContent>
{
    private static Regex FormatPatternRegex { get; } = CreateFormatPatternRegex();
    
    private List<IComponent>? _decomposed;
    private Language? _decomposedWith;
    
    public IContentType<TranslatableContent> Type => MiaContentTypes.Translatable;
    public string Format { get; set; }
    public string? Fallback { get; set; }
    public ICollection<IComponent> With { get; set; } = new List<IComponent>();
    public TranslatableContent(string format, string? fallback, params IComponent[] texts)
    {
        Format = format;
        Fallback = fallback;
        
        foreach (var t in texts)
        {
            With.Add(t);
        }
    }

    public TranslatableContent AddWith(IComponent text)
    {
        With.Add(text);
        return this;
    }
    
    public TranslatableContent Clone() => new(Format, Fallback, With.Select(t => (IComponent) t.Clone()).ToArray());

    private List<IComponent> Decompose(IStyle style)
    {
        var language = Language.Instance;
        if (_decomposedWith == language && _decomposed != null) return _decomposed;
        _decomposedWith = language;
        
        var offset = 0;
        var counter = 0;
        
        var fmt = Fallback != null
            ? language.GetOrDefault(Format, Fallback)
            : language.GetOrDefault(Format);
        
        var matches = FormatPatternRegex.Matches(fmt);
        var parameters = With.ToList();
        
        var result = new List<IComponent>();
        foreach (Match m in matches)
        {
            var c = m.Groups[1].Value;
            var p = m.Groups[2].Value;

            var front = fmt[offset..m.Index];
            if (front.Length > 0) 
                result.Add(new GenericMutableComponent(new LiteralContent(front), style.Clear()));

            offset = m.Index + m.Length;

            if (p == "%" && m.Value == "%%")
            {
                result.Add(new GenericMutableComponent(new LiteralContent(p), style.Clear()));
                continue;
            }

            if (p.ToLowerInvariant() != "s")
            {
                throw new FormatException($"Unsupported format: {p}");
            }

            var ci = c.Length == 0 ? counter++ : int.Parse(c) - 1;
            result.Add(ci >= parameters.Count && ci < 0
                ? new GenericMutableComponent(new LiteralContent(m.Value), style.Clear())
                : parameters[ci].Clone());
        }
        
        result.Add(new GenericMutableComponent(new LiteralContent(fmt[offset..]), style.Clear()));
        _decomposed = result;
        return result;
    }

    public void Visit(IContentVisitor visitor, IStyle style)
    {
        _decomposed ??= Decompose(style);
        foreach (var component in _decomposed)
        {
            component.Visit(visitor, style);
        }
    }
    
    public void VisitLiteral(IContentVisitor visitor, IStyle style)
    {
        _decomposed ??= Decompose(style);
        foreach (var component in _decomposed)
        {
            component.VisitLiteral(visitor, style);
        }
    }

    [GeneratedRegex("%(?:(?:(\\d+?)\\$)?)([A-Za-z%]|$)")]
    private static partial Regex CreateFormatPatternRegex();
}

public class TranslatableContentType : IContentType<TranslatableContent>
{
    public const string Key = "translate";
    
    public TranslatableContent CreateContent(JsonObject payload)
    {
        var key = payload[Key]!.GetValue<string>();
        var fallback = payload["fallback"]?.GetValue<string>();
        var with = payload.TryGetPropertyValue("with", out var withNode) ? withNode!.AsArray() : new JsonArray();
        return new TranslatableContent(key, fallback, with.Select(Component.FromJson).ToArray());
    }

    public void InsertPayload(JsonObject target, TranslatableContent content)
    {
        target[Key] = content.Format;
        
        if (content.Fallback != null)
            target["fallback"] = content.Fallback;
        
        if (!content.With.Any()) return;
        
        var arr = new JsonArray();
        foreach (var text in content.With.Select(t => t.ToJson()))
        {
            arr.Add(text);
        }

        target["with"] = arr;
    }
}