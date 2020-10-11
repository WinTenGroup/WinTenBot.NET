namespace Zizi.Bot.Models.Settings
{
    public class HangfireConfig
    {
        public const string Name = "Hangfire";

        public string BaseUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Mysql { get; set; }
        public string LiteDb { get; set; }
    }
}
