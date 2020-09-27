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


        // Source: https://www.codeproject.com/Articles/44274/Transpose-a-DataTable-using-C
        public static DataTable TransposedTable(this DataTable inputTable)
        {
            DataTable outputTable = new DataTable();

            // Add columns by looping rows

            // Header row's first column is same as in inputTable
            outputTable.Columns.Add(inputTable.Columns[0].ColumnName);

            // Header row's second column onwards, 'inputTable's first column taken
            foreach (DataRow inRow in inputTable.Rows)
            {
                string newColName = inRow[0].ToString();
                outputTable.Columns.Add(newColName);
            }

            // Add rows by looping columns        
            for (int rCount = 1; rCount <= inputTable.Columns.Count - 1; rCount++)
            {
                DataRow newRow = outputTable.NewRow();

                // First column is inputTable's Header row's second column
                newRow[0] = inputTable.Columns[rCount].ColumnName;
                for (int cCount = 0; cCount <= inputTable.Rows.Count - 1; cCount++)
                {
                    string colValue = inputTable.Rows[cCount][rCount].ToString();
                    newRow[cCount + 1] = colValue;
                }

                outputTable.Rows.Add(newRow);
            }

            return outputTable;
        }
    }
}