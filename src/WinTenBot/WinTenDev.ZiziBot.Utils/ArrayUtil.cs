using System.Collections.Generic;
using System.Linq;

namespace WinTenDev.ZiziBot.Utils
{
    public static class ArrayUtil
    {
        public static IEnumerable<(T item, int index)> IterWithIndex<T>(this IEnumerable<T> self)
        {
            return self.Select((item, index) => (item, index));
        }
    }
}