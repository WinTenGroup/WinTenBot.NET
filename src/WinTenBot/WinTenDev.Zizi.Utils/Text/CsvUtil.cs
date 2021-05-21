using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;

namespace WinTenDev.Zizi.Utils.Text
{
    /// <summary>
    /// The csv util.
    /// </summary>
    public static class CsvUtil
    {
        /// <summary>
        /// Writes the.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="records">The records.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="append">If true, append.</param>
        public static void WriteRecords<T>(this List<T> records, string filePath, string delimiter = ",", bool append = false)
        {
            Log.Information("Writing {0} rows to {1}", records.Count, filePath);

            // v20 or above
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = false
            };

            var writer = new StreamWriter(filePath);

            if (append)
            {
                var stream = File.Open(filePath, FileMode.Append);
                writer = new StreamWriter(stream);
            }

            var csv = new CsvWriter(writer, config);

            csv.WriteRecords(records);
            Log.Debug("CSV file written to {0}", filePath);

            writer.Dispose();
            csv.Dispose();
        }

        /// <summary>
        /// Write single record.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="hasHeader"></param>
        /// <returns>A string.</returns>
        public static string WriteRecord<T>(this T record, string filePath, string delimiter = ",", bool hasHeader = false)
        {
            Log.Information("Writing record to {0}", filePath);

            // v20 or above
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = hasHeader
            };

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);

            csv.WriteRecord(record);
            Log.Debug("CSV file written to {0}", filePath);

            return filePath;
        }

        /// <summary>
        /// Appends the row.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>A string.</returns>
        public static string AppendRecord<T>(this T row, string filePath, string delimiter = ",")
        {
            Log.Information("Append record to {0}", filePath);

            // v20 or above
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = false,
            };

            using var stream = File.Open(filePath, FileMode.Append);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);

            csv.WriteField(row);

            Log.Debug("CSV file written to {0}", filePath);

            return filePath;
        }

        /// <summary>
        /// Reads the csv.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="hasHeader">If true, has header.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>A list of TS.</returns>
        public static IEnumerable<T> ReadCsv<T>(string filePath, bool hasHeader = true, string delimiter = ",")
        {
            if (!File.Exists(filePath))
            {
                Log.Information("File {0} is not exist", filePath);
                return new List<T>();
            }

            // v20 or above
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                HasHeaderRecord = hasHeader,
                Delimiter = delimiter,
                MissingFieldFound = null,
                BadDataFound = null,
                PrepareHeaderForMatch = (header) => header.Header.ToLower()
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, csvConfiguration);

            var records = csv.GetRecords<T>().ToList();
            Log.Information("Parsing csv records {0} record(s)", records.Count);

            return records;
        }
    }
}