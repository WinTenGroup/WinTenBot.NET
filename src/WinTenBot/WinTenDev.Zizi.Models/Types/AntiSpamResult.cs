namespace WinTenDev.Zizi.Models.Types
{
    public class AntiSpamResult
    {
        public string MessageResult { get; set; }
        public bool IsAnyBanned { get; set; }
        public bool IsEs2Banned { get; set; }
        public bool IsCasBanned { get; set; }
        public bool IsSpamWatched { get; set; }
    }
}