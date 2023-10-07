using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mochi.Texts;

namespace MiaCrate.Texts;

public static class MiaComponent
{
    public static IMutableComponent<Style> Literal(string message) => 
        new MutableComponent<Style>(new LiteralContent(message), Style.Empty);

    public static IMutableComponent<Style> Translatable(string message, params object[] parameters) =>
        new MutableComponent<Style>(
            new TranslatableContent(message, null, FromObjects(parameters)), Style.Empty);
    
    public static IMutableComponent<Style> TranslatableWithFallback(string message, string fallback, params object[] parameters) =>
        new MutableComponent<Style>(
            new TranslatableContent(message, fallback, FromObjects(parameters)), Style.Empty);
    
    private static IComponent[] FromObjects(object[] parameters)
    {
        return parameters.Select(p =>
        {
            if (p is IComponent comp) return comp;
            if (p is string str) return Literal(str);
            return Literal(p?.ToString() ?? "<null>");
        }).ToArray();
    }

    private static Style ParseStyle(JsonObject o)
    {
        T? GetValueFrom<T>(string key, Func<JsonNode, T> extract) where T : class => 
            o.TryGetPropertyValue(key, out var node) ? extract(node!) : null;
        T? GetNullableValueFrom<T>(string key, Func<JsonNode, T> extract) where T : struct => 
            o.TryGetPropertyValue(key, out var node) ? extract(node!) : null;

        var color = GetValueFrom("color", n =>
        {
            var name = n.GetValue<string>();
            if (name == "light_purple") name = "purple";
            return TextColor.Of(name);
        });
        
        var bold = GetNullableValueFrom("bold", n => n.GetValue<bool>());
        var italic = GetNullableValueFrom("italic", n => n.GetValue<bool>());
        var underline = GetNullableValueFrom("underline", n => n.GetValue<bool>());
        var strikethrough = GetNullableValueFrom("strikethrough", n => n.GetValue<bool>());
        var obfuscated = GetNullableValueFrom("obfuscated", n => n.GetValue<bool>());

        return Style.Empty
            .WithColor(color)
            .WithBold(bold)
            .WithItalic(italic)
            .WithUnderlined(underline)
            .WithStrikethrough(strikethrough)
            .WithObfuscated(obfuscated);
    }
    
    public static IMutableComponent? FromJson(JsonNode? obj)
    {
        if (obj == null) return null;
        
        if (obj is JsonValue value)
            return FromLegacyText(value.GetValue<string>());

        if (obj is JsonArray arr)
        {
            return Literal("")
                .AddExtra(arr
                    .Select(n => (IComponent) FromJson(n)!)
                    .Where(n => n != null!)
                    .ToArray());
        }
        
        if (obj is JsonObject o)
        {
            var content = TextContentTypes.CreateContent(o);
            var t = new MutableComponent<Style>(content, ParseStyle(o));
            var extra = o.TryGetPropertyValue("extra", out var extraNode) ? extraNode!.AsArray() : new JsonArray();
            return t.AddExtra(extra
                .Select(n => (IComponent) FromJson(n)!)
                .Where(n => n != null!)
                .ToArray());
        }
        
        throw new ArgumentException("Invalid JSON");
    }

    public static IMutableComponent? FromJson(string json) => 
        FromJson(JsonNode.Parse(json));

    public static IMutableComponent FromLegacyText(string message)
    {
        var texts = new List<IComponent<Style>>();
        var sb = new StringBuilder();
        var t = Literal("");

        for (var i = 0; i < message.Length; i++)
        {
            var c = message[i];
            if (c == ChatFormatting.PrefixCode)
            {
                if (++i >= message.Length) break;
                c = message[i];

                // lower case
                if (c >= 'A' && c <= 'Z') c += (char)32;

                ChatFormatting? formatting;
                TextColor? color;
                var isHex = false;
                
                if (c == 'x' && i + 12 < message.Length)
                {
                    formatting = null;
                    isHex = true;
                    
                    StringBuilder hex = new("#");
                    for (var j = 0; j < 6; j++)
                    {
                        hex.Append(message[i + 2 + j * 2]);
                    }

                    try
                    {
                        color = TextColor.Of(hex.ToString());
                    }
                    catch (ArgumentException)
                    {
                        color = null;
                    }
                }
                else
                {
                    formatting = ChatFormatting.GetByCode(c);
                    
                    if (formatting is { IsColor: true })
                    {
                        color = formatting.ToTextColor();
                    }
                    else
                    {
                        color = null;
                    }
                }
                
                if (!isHex && formatting == null) continue;

                // push old text to the list
                if (sb.Length > 0)
                {
                    var old = t;
                    t = old.Clone();
                    
                    ((LiteralContent) old.Content).Text = sb.ToString();
                    sb.Clear();
                    texts.Add(old);
                }

                if (formatting == ChatFormatting.Bold)
                {
                    t.SetBold(true);
                } 
                else if (formatting == ChatFormatting.Italic)
                {
                    t.SetItalic(true);
                }
                else if (formatting == ChatFormatting.Underline)
                {
                    t.SetUnderlined(true);
                }
                else if (formatting == ChatFormatting.Strikethrough)
                {
                    t.SetStrikethrough(true);
                }
                else if (formatting == ChatFormatting.Obfuscated)
                {
                    t.SetObfuscated(true);
                }
                else
                {
                    t = Literal("").SetColor(color);
                }
                continue;
            }

            sb.Append(c);
        }

        ((LiteralContent) t.Content).Text = sb.ToString();
        texts.Add(t);
        return texts.ToFlattened().Clone();
    }
}