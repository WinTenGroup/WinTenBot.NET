using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;

namespace Zizi.Bot.Tools.Ocr
{
    public static class OcrSpace
    {
        public static async Task<string> ScanImage(string filePath)
        {
            var result = "";
            try
            {
                await using var fs = File.OpenRead(filePath);
                var url = "https://api.ocr.space/Parse/Image";
                var ocrKey = BotSettings.OcrSpaceKey;
                var fileName = Path.GetFileName(filePath);

                if (ocrKey.IsNullOrEmpty())
                {
                    Log.Warning("OCR can't be continue because API KEY is missing.");
                    return string.Empty;
                }

                Log.Information($"Sending {filePath} to {url}");
                var postResult = await url
                    .PostMultipartAsync(post =>
                        post.AddFile("image", fs, fileName)
                            .AddString("apikey", ocrKey)
                            .AddString("language", "eng"))
                    .ConfigureAwait(false);

                Log.Information($"OCR: {postResult.StatusCode}");
                var json = await postResult.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                var map = JsonConvert.DeserializeObject<OcrResult>(json);

                if (map.OcrExitCode == 1)
                {
                    result = map.ParsedResults.Aggregate(result, (current, t) =>
                        current + t.ParsedText);
                }

                Log.Information("Scan complete.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error OCR Space");
            }

            return result;
        }
    }
}