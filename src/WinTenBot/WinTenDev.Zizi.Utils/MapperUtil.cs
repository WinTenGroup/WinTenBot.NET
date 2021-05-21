using System.Collections.Generic;
using Slapper;

namespace WinTenDev.Zizi.Utils
{
    public static class MapperUtil
    {
        public static T DictionaryMapper<T>(this Dictionary<string, object> dictionary)
        {
            return AutoMapper.Map<T>(dictionary);
        }
    }
}