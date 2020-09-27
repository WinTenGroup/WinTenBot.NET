namespace Zizi.Bot.Model
{
    public class RequestResult
    {
        public bool IsSuccess { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}