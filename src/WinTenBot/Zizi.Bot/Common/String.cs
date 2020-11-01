using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using Serilog;

namespace Zizi.Bot.Common
{
    public static class String
    {
        public static List<string> SplitText(this string text, string delimiter)
        {
            return text.Split(delimiter).ToList();
        }

        public static string ResolveVariable(this string input, object parameters)
        {
            Log.Information("Resolving variable..");
            var type = parameters.GetType();
            Regex regex = new Regex("\\{(.*?)\\}");
            var sb = new StringBuilder();
            var pos = 0;

            if (input == null) return input;

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
            Log.Information($"Writing file to {path}");
            await File.WriteAllTextAsync(path, content);

            //            var sw = new StreamWriter(path);
            //            sw.Write(content);
            //            sw.Close();
            //            sw.Dispose();

            //            using (var sw = new StreamWriter(@path))
            //            {
            //                await sw.WriteAsync(content);
            //                sw.Close();
            //                sw.Dispose();
            //            }

            //            var buffer = Encoding.UTF8.GetBytes(content);
            //
            //            using (var fs = new FileStream(@path, FileMode.OpenOrCreate, 
            //                FileAccess.Write, FileShare.None, buffer.Length, true))
            //            {
            //                await fs.WriteAsync(buffer, 0, buffer.Length);
            //                fs.Close();
            //            }
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
            return Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp);
        }

        public static string GetBaseUrl(this string url)
        {
            var uri = new Uri(url);
            return uri.Host;
        }

        public static async Task<string> GetUrlRssFeed(string url)
        {
            var urls = await FeedReader.GetFeedUrlsFromUrlAsync(url);

            string feedUrl = url;
            if (urls.Count() < 1) // no url - probably the url is already the right feed url
                feedUrl = url;
            else if (urls.Count() == 1)
                feedUrl = urls.First().Url;
            else if (urls.Count() == 2
            ) // if 2 urls, then its usually a feed and a comments feed, so take the first per default
                feedUrl = urls.First().Url;

            return feedUrl;
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
            return System.String.Join(delim, obj.ToArray());
        }

        public static string CleanExceptAlphaNumeric(this string str)
        {
            var arr = str.Where(c => (char.IsLetterOrDigit(c) ||
                                      char.IsWhiteSpace(c) ||
                                      c == '-')).ToArray();

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
            foreach (var remove in forRemoves)
            {
                str = str.Replace(remove, "").Trim();
            }

            return str;
        }

        public static string StripMargin(this string s)
        {
            return Regex.Replace(s, @"[ \t]+\|", string.Empty);
        }

        public static string StripLeadingWhitespace(this string s)
        {
            Regex r = new Regex(@"^\s+", RegexOptions.Multiline);
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
            return new string(str.ToCharArray().Distinct().ToArray());
        }
        
        public static string SizeFormat(this double size, string suffix = null)
        {
            string[] norm = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int count = norm.Length - 1;
            // var size = bytes;
            int x = 0;

            while (size >= 1000 && x < count)
            {
                size /= 1024;
                x++;
            }

            return string.Format($"{Math.Round(size, 2)} {norm[x]}{suffix}", MidpointRounding.AwayFromZero);
        }

        public static string GenerateUniqueId()
        {
            var builder = new StringBuilder();
            Enumerable
                .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => Guid.NewGuid())
                .Take(11)
                .ToList()
                .ForEach(e => builder.Append(e));
            
            var id = builder.ToString();

            return id;
        }
    }
}