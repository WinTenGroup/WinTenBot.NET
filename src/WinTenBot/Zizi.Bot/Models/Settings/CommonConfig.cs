namespace Zizi.Bot.Models.Settings
{
    public class CommonConfig
    {
        public string ChannelLogs { get; set; }
        public string ConnectionString { get; set; }
        public string MysqlDbName { get; set; }
        public string SpamWatchToken { get; set; }
        public bool IsRestricted { get; set; }
    }
}