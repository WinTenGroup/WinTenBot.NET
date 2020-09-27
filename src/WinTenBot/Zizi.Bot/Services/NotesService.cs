using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Providers;
using Zizi.Bot.Model;

namespace Zizi.Bot.Services
{
    public class NotesService
    {
        private string baseTable = "notes";
        
        public async Task<DataTable> GetNotesByChatId(long chatId)
        {
            // var sql = $"SELECT * FROM {baseTable} WHERE chat_id = '{chatId}'";
            // var data = await _mySql.ExecQueryAsync(sql);
            
            var query = await new Query(baseTable)
                .Where("chat_id",chatId)
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false);

            var data = query.ToJson().MapObject<DataTable>();
            return data;
        }

        public async Task<List<CloudNote>> GetNotesBySlug(long chatId, string slug)
        {
            Log.Information("Getting Notes by Slug..");
            
            var query = await new Query(baseTable)
                .Where("chat_id",chatId)
                .OrWhereContains("slug",slug)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);

            var mapped = query.ToJson().MapObject<List<CloudNote>>();
            return mapped;
            
            // var sql = $"SELECT * FROM {baseTable} WHERE chat_id = '{chatId}' " +
            // $"AND MATCH(slug) AGAINST('{slug.SqlEscape()}')";
            // var data = await _mySql.ExecQueryAsync(sql);
            // return data;
        }

        public async Task SaveNote(Dictionary<string, object> data)
        {
            var json = data.ToJson();
            Log.Information(json);

            var insert = await new Query(baseTable)
                .ExecForMysql()
                .InsertAsync( data)
                .ConfigureAwait(false);
            
            Log.Information($"SaveNote: {insert}");
        }

        public async Task UpdateCache(long chatId)
        {
            var data = await GetNotesByChatId(chatId)
                .ConfigureAwait(false);
            await data.WriteCacheAsync($"{chatId}/notes.json")
                .ConfigureAwait(false);
        }
    }
}