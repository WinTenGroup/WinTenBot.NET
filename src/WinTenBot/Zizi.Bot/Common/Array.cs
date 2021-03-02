using System.Collections.Generic;
using System.Data;
using System.Linq;
using Serilog;

namespace Zizi.Bot.Common
{
    public static class Array
    {
        public static T[][] ChunkBy<T>(this IEnumerable<T> btnList, int chunk = 2)
        {
            Log.Information($"Chunk buttons to {chunk}");
            var chunksBtn = btnList
                .Select((s, i) => new {Value = s, Index = i})
                .GroupBy(x => x.Index / chunk)
                .Select(grp => grp.Select(x => x.Value).ToArray())
                .ToArray();

            return chunksBtn;
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize = 2)
        {
            return source
                .Select((x, i) => new {Index = i, Value = x})
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public static bool IsNullIndex(this object[] array, int index)
        {
            if (array.Length <= index) return false;

            return array[index] != null;
        }

        public static string ValueOfIndex(this string[] array, int index)
        {
            string value = null;
            if (array.Length > index && array[index] != null)
            {
                value = array[index];
                Log.Debug($"Get Array index {index}: {value}");
            }

            return value;
        }

        public static T[,] TransposeMatrix<T>(this T[,] matrix)
        {
            var rows = matrix.GetLength(0);
            var columns = matrix.GetLength(1);

            var result = new T[columns, rows];

            for (var c = 0; c < columns; c++)
            {
                for (var r = 0; r < rows; r++)
                {
                    result[c, r] = matrix[r, c];
                }
            }

            return result;
        }
    }
}