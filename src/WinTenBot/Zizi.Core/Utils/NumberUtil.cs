using System;
using System.Globalization;

namespace Zizi.Core.Utils
{
    public static class NumberUtil
    {
        public static string SizeFormat(this double size, string suffix = null)
        {
            string[] norm = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            int count = norm.Length - 1;
            int x = 0;

            while (size >= 1000 && x < count)
            {
                size /= 1024;
                x++;
            }

            return string.Format($"{Math.Round(size, 2):N} {norm[x]}{suffix}", MidpointRounding.ToZero);
        }

        public static string SizeFormat(this long size, string suffix = null)
        {
            var sizeD = Convert.ToDouble(size);
            return SizeFormat(sizeD, suffix);

        }

        public static string NumberSeparator(this int number)
        {
            return number.ToString("N0", new CultureInfo("id-ID"));
        }
    }
}