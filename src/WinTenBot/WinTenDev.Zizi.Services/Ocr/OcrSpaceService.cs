using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using Serilog;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services.Ocr
{
    public class OcrSpaceService
    {
        public async Task<string> ScanImage(string filePath)
        {
            var result = "";
            try
            {
                await using var fs = File.OpenRead(filePath);
                var url = "https://api.ocr.space/Parse/Image";
                var ocrKey = "BotSettings.OcrSpaceKey";
                var fileName = Path.GetFileName(filePath);

                if (ocrKey.IsNullOrEmpty())
                {
                    Log.Warning("OCR can't be continue because API KEY is missing.");
                    return string.Empty;
                }

                Log.Information("Sending {FilePath} to {Url}", filePath, url);
                var postResult = await url
                    .PostMultipartAsync(post =>
                        post.AddFile("image", fs, fileName)
                            .AddString("apikey", ocrKey)
                            .AddString("language", "eng"));

                Log.Information("OCR: {StatusCode}", postResult.StatusCode);
                var json = await postResult.GetStringAsync();

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