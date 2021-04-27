using System.Collections.Generic;
using System.Linq;

namespace WinTenDev.Commons.Utils
{
    public static class ArrayUtil
    {
        public static IEnumerable<(T item, int index)> IterWithIndex<T>(this IEnumerable<T> self)
            => self.Select((item, index) => (item, index));
    }
}