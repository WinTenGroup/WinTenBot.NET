namespace WinTenDev.Zizi.Models.Configs
{
    public class DatabaseConfig
    {
        public string MysqlBase { get; set; }
        public string MysqlDb { get; set; }
        public string MysqlHangfireDb { get; set; }

        public string MysqlDataConn => $"{MysqlBase}Database={MysqlDb}";
        public string MysqlHangfireConn => $"{MysqlBase}Database={MysqlHangfireDb}";
    }
}