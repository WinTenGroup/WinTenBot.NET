using Serilog;
using Tesseract;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services.Ocr
{
    public class OcrTesseractService
    {
        public string ScanImage(string fileName)
        {
            string result = "";
            Log.Information("Scanning {FileName}", fileName);
            using (var engine = new TesseractEngine(BotSettings.TesseractTrainedData, "ind", EngineMode.TesseractOnly))
            {
                using var img = Pix.LoadFromFile(fileName);
                var page = engine.Process(img, PageSegMode.Auto);
                result = page.GetText();
                Log.Information("Scan complete.");
            }

            return result.IsNullOrEmpty() ? "Return empty." : result;
        }
    }
}