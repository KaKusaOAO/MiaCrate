using System.Text.RegularExpressions;

namespace MiaCrate;

public static class StringHelper
{
    private static readonly Regex _lineEndRegex = new("(?:\r\n|\\v)$");
    private static readonly Regex _lineRegex = new("(?:\r\n|\\v)$");

    public static bool EndsWithNewLine(this string str) => _lineEndRegex.IsMatch(str);

    public static int GetLineCount(this string str) => string.IsNullOrEmpty(str) ? 0 : _lineRegex.Matches(str).Count;
}