using System.Threading.Tasks;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.Models;

namespace Zizi.Bot.Tools
{
    public static class BackupUtil
    {
        public static async Task<string> BackupAllTags(this QueryFactory factory)
        {
            var tags = await factory.FromQuery(new Query("tags"))
                .GetAsync<CloudTag>();

            var filePath = await tags.WriteToFileAsync("all-tags.json");

            return filePath;
        }
    }
}