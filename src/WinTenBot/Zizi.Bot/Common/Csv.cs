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
            Log.Information($"Writing {records.Count()} rows to {filePath}");

            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                Delimiter = delimiter,
            };

            using var writer = new StreamWriter(filePath);
            using var csv = new CsvWriter(writer, config);
            csv.Context.WriterConfiguration.HasHeaderRecord = false;
            csv.WriteRecords(records);
        }

        public static IEnumerable<T> ReadCsv<T>(string filePath, bool hasHeader = true, string delimiter = ",")
        {
            if (!File.Exists(filePath))
            {
                Log.Information($"File {filePath} is not exist");
                return null;
            }

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            csv.Configuration.HasHeaderRecord = hasHeader;
            csv.Configuration.Delimiter = delimiter;
            csv.Configuration.MissingFieldFound = null;
            csv.Configuration.BadDataFound = null;
            // csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.ToLower();

            var records = csv.GetRecords<T>().ToList();
            Log.Information($"Parsing csv records {records.Count} row(s)");

            return records;
        }
    }
}