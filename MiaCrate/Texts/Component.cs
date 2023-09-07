using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mochi.Texts;

namespace MiaCrate.Texts;

public static class Component
{
    public static IMutableComponent<Style> Literal(string message) => 
        Mochi.Texts.Component.Literal(message, Style.Empty);

    private static Style ParseStyle(JsonObject o)
    {
        T? GetValueFrom<T>(string key, Func<JsonNode, T> extract) where T : class => 
            o.TryGetPropertyValue(key, out var node) ? extract(node!) : null;
        T? GetNullableValueFrom<T>(string key, Func<JsonNode, T> extract) where T : struct => 
            o.TryGetPropertyValue(key, out var node) ? extract(node!) : null;

        var color = GetValueFrom("color", n => TextColor.Of(n.GetValue<string>()));
        var bold = GetNullableValueFrom("bold", n => n.GetValue<bool>());
        var italic = GetNullableValueFrom("italic", n => n.GetValue<bool>());
        var underline = GetNullableValueFrom("underline", n => n.GetValue<bool>());
        var strikethrough = GetNullableValueFrom("strikethrough", n => n.GetValue<bool>());
        var obfuscated = GetNullableValueFrom("obfuscated", n => n.GetValue<bool>());

        return Style.Empty.WithColor(color)
            .WithBold(bold).WithItalic(italic)
            .WithUnderline(underline).WithStrikethrough(strikethrough)
            .WithObfuscated(obfuscated);
    }
    
    public static IMutableComponent FromJson(JsonNode? obj)
    {
        if (obj is JsonValue value)
        {
            return FromLegacyText(value.GetValue<string>());
        }
        
        if (obj is JsonArray arr)
        {
            return Literal("")
                .AddExtra(arr.Select(n => (IComponent) FromJson(n)).ToArray());
        }
        
        if (obj is JsonObject o)
        {
            var content = TextContentTypes.CreateContent(o);
            var t = new MutableComponent<Style>(content, ParseStyle(o));
            var extra = o.TryGetPropertyValue("extra", out var extraNode) ? extraNode!.AsArray() : new JsonArray();
            return t.AddExtra(extra.Select(n => (IComponent) FromJson(n)).ToArray());
        }
        
        throw new ArgumentException("Invalid JSON");
    }

    public static IMutableComponent FromJson(string json) => 
        FromJson(JsonSerializer.Deserialize<JsonNode>(json));

    public static IMutableComponent FromLegacyText(string message)
    {
        var texts = new List<IComponent<Style>>();
        var sb = new StringBuilder();
        var t = Literal("");

        for (var i = 0; i < message.Length; i++)
        {
            var c = message[i];
            if (c == '\u00a7')
            {
                if (++i >= message.Length) break;
                c = message[i];

                // lower case
                if (c >= 'A' && c <= 'Z') c += (char)32;

                ChatColor? color;
                if (c == 'x' && i + 12 < message.Length)
                {
                    StringBuilder hex = new("#");
                    for (var j = 0; j < 6; j++)
                    {
                        hex.Append(message[i + 2 + j * 2]);
                    }

                    try
                    {
                        color = ChatColor.Of(hex.ToString());
                    }
                    catch (ArgumentException)
                    {
                        color = null;
                    }
                }
                else
                {
                    color = ChatColor.Of(c);
                }
                
                if (color == null) continue;

                // push old text to the list
                if (sb.Length > 0)
                {
                    var old = t;
                    t = old.Clone();
                    
                    ((LiteralContent) old.Content).Text = sb.ToString();
                    sb.Clear();
                    texts.Add(old);
                }

                if (color == ChatColor.Bold)
                {
                    t.SetBold(true);
                } 
                else if (color == ChatColor.Italic)
                {
                    t.SetItalic(true);
                }
                else if (color == ChatColor.Underline)
                {
                    t.SetUnderline(true);
                }
                else if (color == ChatColor.Strikethrough)
                {
                    t.SetStrikethrough(true);
                }
                else if (color == ChatColor.Obfuscated)
                {
                    t.SetObfuscated(true);
                }
                else
                {
                    var tc = color.Color.HasValue ? TextColor.Of(color.Color!.Value) : null;
                    if (color == ChatColor.Reset) tc = null;
                    t = Literal("").SetColor(tc);
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