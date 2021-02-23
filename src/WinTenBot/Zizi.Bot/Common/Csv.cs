using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Serilog;

namespace Zizi.Bot.Common
{
    public static class Csv
    {
        public static void Write<T>(string filePath, IEnumerable<T> records, string delimiter = ",")
        {
            var listRecord = records.ToList();
            Log.Information("Writing {0} rows to {1}", listRecord.Count, filePath);

            // v20 or above
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
                HasHeaderRecord = false
            };

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);

            // v19 or below
            // csv.Context.WriterConfiguration.HasHeaderRecord = false;

            csv.WriteRecords(listRecord);
            Log.Debug("CSV file written to {0}", filePath);
        }

        public static IEnumerable<T> ReadCsv<T>(string filePath, bool hasHeader = true, string delimiter = ",")
        {
            if (!File.Exists(filePath))
            {
                Log.Information("File {0} is not exist", filePath);
                return null;
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

            // v19 or below
            // csv.Configuration.HasHeaderRecord = hasHeader;
            // csv.Configuration.Delimiter = delimiter;
            // csv.Configuration.MissingFieldFound = null;
            // csv.Configuration.BadDataFound = null;
            // csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();

            var records = csv.GetRecords<T>().ToList();
            Log.Information("Parsing csv records {0} row(s)", records.Count);

            return records;
        }
    }
}