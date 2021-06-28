namespace WinTenDev.Zizi.Models.Types
{
    public class LastError
    {
        public string FullText { get; set; }
        public string[] ErrorLines { get; set; }
        public string[] ErrorSplit { get; set; }
    }
}