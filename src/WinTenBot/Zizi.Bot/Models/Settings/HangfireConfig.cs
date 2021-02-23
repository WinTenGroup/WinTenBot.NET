namespace Zizi.Bot.Models.Settings
{
    public class HangfireConfig
    {
        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int WorkerMultiplier { get; set; }
        public string MysqlDbName { get; set; }
        public string Mysql { get; set; }
        public string LiteDb { get; set; }
    }
}