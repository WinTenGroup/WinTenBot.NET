namespace Zizi.Bot.Models
{
    public class OcrResult
    {
        public ParsedResult[] ParsedResults { get; set; }
        public int OcrExitCode { get; set; }
        public bool IsErrorOnProcessing { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class ParsedResult
    {
        public object FileParseExitCode { get; set; }
        public string ParsedText { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
    }
}