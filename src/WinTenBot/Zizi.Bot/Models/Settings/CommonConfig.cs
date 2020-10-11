namespace Zizi.Bot.Models.Settings
{
    public class CommonConfig
    {
        public const string Name = "CommonConfig";

        public string ChannelLogs { get; set; }
        public string SpamWatchToken { get; set; }
        public bool IsRestricted { get; set; }
    }
}
