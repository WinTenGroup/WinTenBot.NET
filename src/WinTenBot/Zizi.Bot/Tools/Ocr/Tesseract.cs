using Serilog;
using Tesseract;
using Zizi.Bot.Common;
using Zizi.Bot.Models;

namespace Zizi.Bot.Tools.Ocr
{
    public static class Tesseract
    {
        public static string ScanImage(string fileName)
        {
            string result = "";
            Log.Information($"Scanning {fileName}");
            using (var engine = new TesseractEngine(BotSettings.TesseractTrainedData, "ind",
                EngineMode.TesseractOnly))
            {
                using (var img = Pix.LoadFromFile(fileName))
                {
                    var page = engine.Process(img, PageSegMode.Auto);
                    result = page.GetText();
                    Log.Information("Scan complete.");
                }
            }

            return result.IsNullOrEmpty() ? "Return empty." : result;
        }
    }
}