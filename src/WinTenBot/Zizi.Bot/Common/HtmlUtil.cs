using System.Web;

namespace Zizi.Bot.Common
{
    public static class HtmlUtil
    {
        public static string HtmlEncode(this string html)
        {
            return HttpUtility.HtmlEncode(html);
        }

        public static string HtmlDecode(this string html)
        {
            return HttpUtility.HtmlDecode(html);
        }
    }
}