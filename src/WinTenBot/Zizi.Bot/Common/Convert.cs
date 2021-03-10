using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Zizi.Bot.Common
{
    public static class Convert
    {
        public static bool ToBool(this object obj)
        {
            return System.Convert.ToBoolean(obj);
        }

        public static bool ToBool(this string obj)
        {
            return System.Convert.ToBoolean(obj);
        }
        
        public static double ToDouble(this long num)
        {
            return System.Convert.ToDouble(num);
        }

        public static int ToInt(this object obj)
        {
            return System.Convert.ToInt32(obj);
        }

        public static long ToInt64(this object obj)
        {
            return System.Convert.ToInt64(obj);
        }

        public static int ToBoolInt(this string str)
        {
            return str.ToLowerCase() == "disable" ? 0 : 1;
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> ts) where T : class
        {
            var dt = new DataTable();
            //Get Enumerable Type
            Type tT = typeof(T);

            //Get Collection of NoVirtual properties
            var props = tT.GetProperties().Where(p => !p.GetGetMethod().IsVirtual).ToArray();

            //Fill Schema
            foreach (PropertyInfo p in props)
                dt.Columns.Add(p.Name, p.GetMethod.ReturnParameter.ParameterType.BaseType);

            //Fill Data
            foreach (T t in ts)
            {
                DataRow row = dt.NewRow();

                foreach (PropertyInfo p in props)
                    row[p.Name] = p.GetValue(t);

                dt.Rows.Add(row);
            }

            return dt;
        }

        public static IEnumerable<DataRow> AsEnumerableX(this DataTable table)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                yield return table.Rows[i];
            }
        }

        public static bool IsNumeric(this string str)
        {
            if (str.IsNullOrEmpty()) return false;
            return str.All(char.IsDigit);
        }
    }
}