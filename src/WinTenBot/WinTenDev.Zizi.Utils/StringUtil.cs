using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;

namespace WinTenDev.Zizi.Utils
{
    public static class StringUtil
    {
        public static List<string> SplitText(this string text, string delimiter)
        {
            return text?.Split(delimiter).ToList();
        }

        public static string ResolveVariable(this string input, object parameters)
        {
            Log.Debug("Resolving variable..");
            var type = parameters.GetType();
            var regex = new Regex("\\{(.*?)\\}");
            var sb = new StringBuilder();
            var pos = 0;

            if (input == null) return null;

            foreach (Match toReplace in regex.Matches(input))
            {
                var capture = toReplace.Groups[0];
                var paramName = toReplace.Groups[toReplace.Groups.Count - 1].Value;
                var property = type.GetProperty(paramName);
                if (property == null) continue;
                sb.Append(input.Substring(pos, capture.Index - pos));
                sb.Append(property.GetValue(parameters, null));
                pos = capture.Index + capture.Length;
            }

            if (input.Length > pos + 1) sb.Append(input.Substring(pos));

            return sb.ToString();
        }

        public static async Task ToFile(this string content, string path)
        {
            Log.Debug("Writing file to {0}", path);
            await File.WriteAllTextAsync(path, content);
        }

        public static string SqlEscape(this string str)
        {
            if (str.IsNullOrEmpty()) return str;

            var escaped = Regex.Replace(str, @"[\x00'""\b\n\r\t\cZ\\%_]",
                delegate(Match match)
                {
                    var v = match.Value;
                    switch (v)
                    {
                        case "\x00": // ASCII NUL (0x00) character
                            return "\\0";

                        case "\b": // BACKSPACE character
                            return "\\b";

                        case "\n": // NEWLINE (linefeed) character
                            return "\\n";

                        case "\r": // CARRIAGE RETURN character
                            return "\\r";

                        case "\t": // TAB
                            return "\\t";

                        case "\u001A": // Ctrl-Z
                            return "\\Z";

                        default:
                            return "\\" + v;
                    }
                });
            escaped = escaped.Replace("'", "\'");

            return escaped;
        }

        public static string NewSqlEscape(this object obj)
        {
            return obj.ToString().Replace("'", "''");
        }

        public static bool CheckUrlValid(this string source)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp);
        }

        public static string GetBaseUrl(this string url)
        {
            var uri = new Uri(url);
            return uri.Host;
        }

        public static string NumSeparator(this object number)
        {
            return $"{number:#,#.00}";
        }

        public static string MkUrl(this string text, string url)
        {
            return $"<a href ='{url}'>{text}</a>";
        }

        public static string MkJoin(this ICollection<string> obj, string delim)
        {
            return string.Join(delim, obj.ToArray());
        }

        public static string CleanExceptAlphaNumeric(this string str)
        {
            var arr = str.Where(c => char.IsLetterOrDigit(c) ||
                                     char.IsWhiteSpace(c) ||
                                     c == '-').ToArray();

            return new string(arr);
        }

        public static string RemoveThisChar(this string str, string chars)
        {
            return str.IsNullOrEmpty()
                ? str
                : chars.Aggregate(str, (current, c) =>
                    current.Replace($"{c}", ""));
        }

        public static string RemoveThisString(this string str, string[] forRemoves)
        {
            foreach (var remove in forRemoves) str = str.Replace(remove, "").Trim();

            return str;
        }

        public static string RemoveLastLines(this string str, int lines = 0)
        {
            return str.Remove(str.TrimEnd().LastIndexOf(Environment.NewLine, StringComparison.Ordinal) + lines).Trim();
        }

        public static string StripMargin(this string s)
        {
            return Regex.Replace(s, @"[ \t]+\|", string.Empty);
        }

        public static string StripLeadingWhitespace(this string s)
        {
            var r = new Regex(@"^\s+", RegexOptions.Multiline);
            return r.Replace(s, string.Empty);
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !str.IsNullOrEmpty();
        }

        public static bool IsContains(this string str, string filter)
        {
            return str.Contains(filter);
        }

        public static string ToLowerCase(this string str)
        {
            return str.ToLower(CultureInfo.CurrentCulture);
        }

        public static string ToUpperCase(this string str)
        {
            return str.ToUpper(CultureInfo.CurrentCulture);
        }

        public static string ToTitleCase(this string text)
        {
            var textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(text.ToLower());
        }


        public static string RemoveStrAfterFirst(this string str, string after)
        {
            return str.Substring(0, str.IndexOf(after, StringComparison.Ordinal) + 1);
        }

        public static string DistinctChar(this string str)
        {
            return new(str.ToCharArray().Distinct().ToArray());
        }

        public static string GenerateUniqueId(int lengthId = 11)
        {
            var builder = new StringBuilder();
            Enumerable
                .Range(65, 26)
                .Select(e => ((char) e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char) e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(_ => Guid.NewGuid())
                .Take(lengthId)
                .ToList()
                .ForEach(e => builder.Append(e));

            var id = builder.ToString();

            return id;
        }

        public static string ToTrimmedString(this StringBuilder sb)
        {
            return sb.ToString().Trim();
        }
    }
}