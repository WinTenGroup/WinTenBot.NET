using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace WinTenDev.Zizi.Utils
{
    /// <summary>
    /// Extensions method to quick convert any type into other type.
    /// </summary>
    public static class ConvertUtil
    {
        /// <summary>Convert object type to bool.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static bool ToBool(this object obj)
        {
            return Convert.ToBoolean(obj);
        }

        /// <summary>Convert string type to bool.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static bool ToBool(this string obj)
        {
            return Convert.ToBoolean(obj);
        }

        /// <summary>Convert long type to double.</summary>
        /// <param name="num">The number.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static double ToDouble(this long num)
        {
            return Convert.ToDouble(num);
        }

        /// <summary>Convert object type to an integer.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static int ToInt(this object obj)
        {
            return Convert.ToInt32(obj);
        }

        /// <summary>Convert object type to int64.</summary>
        /// <param name="obj">The object.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public static long ToInt64(this object obj)
        {
            return Convert.ToInt64(obj);
        }

        /// <summary>Convert string type to bool int.</summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <br />
        /// </returns>
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

        /// <summary>Check string is numeric.</summary>
        /// <param name="str">The string.</param>
        /// <returns>
        ///   <c>true</c> if the specified string is numeric; otherwise, <c>false</c>.</returns>
        public static bool IsNumeric(this string str)
        {
            if (str.IsNullOrEmpty()) return false;
            return str.All(char.IsDigit);
        }
    }
}