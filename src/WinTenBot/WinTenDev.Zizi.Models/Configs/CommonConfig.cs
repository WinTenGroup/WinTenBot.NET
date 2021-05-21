using WinTenDev.Zizi.Models.Enums;

namespace WinTenDev.Zizi.Models.Configs
{
    public class CommonConfig
    {
        public EngineMode EngineMode { get; set; }
        public bool EnableLocalTunnel { get; set; }
        public string ChannelLogs { get; set; }
        public string ConnectionString { get; set; }
        public string MysqlDbName { get; set; }
        public string SpamWatchToken { get; set; }
        public string DeepAiToken { get; set; }
        public bool IsRestricted { get; set; }
    }
}