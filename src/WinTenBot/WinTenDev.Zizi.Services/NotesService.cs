﻿using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using WinTenDev.Zizi.Models.Types;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Providers;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
{
    public class NotesService
    {
        private readonly string baseTable = "notes";

        public async Task<DataTable> GetNotesByChatId(long chatId)
        {
            // var sql = $"SELECT * FROM {baseTable} WHERE chat_id = '{chatId}'";
            // var data = await _mySql.ExecQueryAsync(sql);

            var query = await new Query(baseTable)
                .Where("chat_id", chatId)
                .ExecForMysql()
                .GetAsync();

            var data = query.ToJson().MapObject<DataTable>();
            return data;
        }

        public async Task<List<CloudNote>> GetNotesBySlug(long chatId, string slug)
        {
            Log.Information("Getting Notes by Slug..");

            var query = await new Query(baseTable)
                .Where("chat_id", chatId)
                .OrWhereContains("slug", slug)
                .ExecForMysql(true)
                .GetAsync();

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
            Log.Information("Json: {0}", json);

            var insert = await new Query(baseTable)
                .ExecForMysql()
                .InsertAsync(data);

            Log.Information("SaveNote: {Insert}", insert);
        }

        public async Task UpdateCache(long chatId)
        {
            var data = await GetNotesByChatId(chatId);
            await data.WriteCacheAsync($"{chatId}/notes.json");
        }
    }
}