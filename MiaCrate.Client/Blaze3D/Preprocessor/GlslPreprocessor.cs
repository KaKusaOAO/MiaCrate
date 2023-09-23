using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MiaCrate.Client.Preprocessor;

public abstract partial class GlslPreprocessor
{
    [StringSyntax("Regex")]
    private const string RegexMojImportPattern =
        "(#(?:/\\*(?:[^*]|\\*+[^*/])*\\*+/|[ \t\xA0\u1680\u180e\u2000-\u200a\u202f\u205f\u3000])*moj_import(?:/\\*(?:[^*]|\\*+[^*/])*\\*+/|[ \t\xA0\u1680\u180e\u2000-\u200a\u202f\u205f\u3000])*(?:\"(.*)\"|<(.*)>))";

    [StringSyntax("Regex")]
    private const string RegexVersionPattern = 
        "(#(?:/\\*(?:[^*]|\\*+[^*/])*\\*+/|[ \t\xA0\u1680\u180e\u2000-\u200a\u202f\u205f\u3000])*version(?:/\\*(?:[^*]|\\*+[^*/])*\\*+/|[ \t\xA0\u1680\u180e\u2000-\u200a\u202f\u205f\u3000])*(\\d+))\\b";

    [StringSyntax("Regex")]
    private const string RegexEndsWithWhitespacePattern =
        "(?:^|[\n\x0B\f\r\x85\u2028\u2029])(?:[ \t\n\x0B\f\r]|/\\*(?:[^*]|\\*+[^*/])*\\*+/|(//[^[\n\x0B\f\r\x85\u2028\u2029]]*))*\\z";

    private static readonly Regex _regexMojImport = GenerateMojImportRegex();
    private static readonly Regex _regexVersion = GenerateVersionRegex();
    private static readonly Regex _regexEndsWithWhitespace = GenerateEndsWithWhitespaceRegex();

    public List<string> Process(string str)
    {
        var context = new Context();
        var list = ProcessImports(str, context, "");
        list[0] = SetVersion(list[0], context.GlslVersion);
        return list.Select(r => r.Replace("\0", "")).ToList();
    }

    private List<string> ProcessImports(string str, Context context, string str2)
    {
        var i = context.SourceId;
        var j = 0;
        var str3 = "";
        var list = new List<string>();
        var matches = _regexMojImport.Matches(str);

        string str4;
        foreach (Match match in matches)
        {
            if (!IsDirectiveDisabled(str, match, j))
            {
                str4 = match.Groups[2].Value;
                var bl = !string.IsNullOrEmpty(str4);
                if (!bl)
                {
                    str4 = match.Groups[3].Value;
                }

                if (!string.IsNullOrEmpty(str4))
                {
                    var str5 = str[j..match.Groups[1].Index];
                    var str6 = str2 + str4;
                    var str7 = ApplyImport(bl, str6);

                    if (!string.IsNullOrEmpty(str7))
                    {
                        if (str7.EndsWithNewLine())
                        {
                            str7 += Environment.NewLine;
                        }

                        var k = ++context.SourceId;
                        var list2 = ProcessImports(str7, context, bl ? FileHelper.GetFullResourcePath(str6) : "");
                        list2[0] = $"#line 0 {k}\n{ProcessVersions(list2[0], context)}";

                        if (!str5.IsBlank())
                        {
                            list.Add(str5);
                        }
                        
                        list.AddRange(list2);
                    }
                    else
                    {
                        var str8 = bl ? $"/*#moj_import \"{str4}\"*/" : $"/*#moj_import <{str4}>*/";
                        list.Add(str3 + str5 + str8);
                    }

                    var end = match.Groups[1].Index + match.Groups[1].Length;
                    var lines = str[..end].GetLineCount();
                    str3 = $"#line {lines} {i}";
                    j = end;
                }
            }
        }

        str4 = str[j..];
        if (!str4.IsBlank())
        {
            list.Add(str3 + str4);
        }

        return list;
    }

    private string ProcessVersions(string str, Context context)
    {
        var matches = _regexVersion.Matches(str);
        if (matches.Any())
        {
            var match = matches.First();
            if (IsDirectiveEnabled(str, match))
            {
                context.GlslVersion = Math.Max(context.GlslVersion, int.Parse(match.Groups[2].Value));
                var end = match.Groups[1].Index + match.Groups[1].Length;
                return str[..match.Groups[1].Index] + "/*" + match.Groups[1].Value + "*/" + str[end..];
            }
        }

        return str;
    }

    private string SetVersion(string str, int i)
    {
        var matches = _regexVersion.Matches(str);
        if (matches.Any())
        {
            var match = matches.First();
            if (IsDirectiveEnabled(str, match))
            {
                var end = match.Groups[2].Index + match.Groups[2].Length;
                return str[..match.Groups[2].Index] + Math.Max(i, int.Parse(match.Groups[2].Value)) + str[end..];
            }
        }

        return str;
    }

    private static bool IsDirectiveEnabled(string str, Match match) => !IsDirectiveDisabled(str, match, 0);
    
    private static bool IsDirectiveDisabled(string str, Match match, int i)
    {
        var j = match.Index - i;
        if (j == 0) return false;

        var matches = _regexEndsWithWhitespace.Matches(str[i..match.Index]);
        if (!matches.Any()) return true;

        var m = matches.First();
        var group = m.Groups[1];
        var k = group.Index + group.Length;
        return k == match.Index;
    }

    public abstract string? ApplyImport(bool flag, string path);

    private class Context
    {
        public int GlslVersion { get; set; }
        public int SourceId { get; set; }
    };

    [GeneratedRegex(RegexMojImportPattern)]
    private static partial Regex GenerateMojImportRegex();
    [GeneratedRegex(RegexVersionPattern)]
    private static partial Regex GenerateVersionRegex();
    [GeneratedRegex(RegexEndsWithWhitespacePattern)]
    private static partial Regex GenerateEndsWithWhitespaceRegex();
}