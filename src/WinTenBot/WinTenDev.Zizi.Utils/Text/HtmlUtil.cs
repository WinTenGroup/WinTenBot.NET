using System.Web;

namespace WinTenDev.Zizi.Utils.Text
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