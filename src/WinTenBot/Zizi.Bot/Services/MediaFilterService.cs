using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Services
{
    public class MediaFilterService
    {
        private string baseTable = "media_filters";
        private string fileJson = "media_filter.json";


        public async Task<bool> IsExist(string key, string value)
        {
            var query = await new Query(baseTable)
                .ExecForMysql(true)
                .Where(key, value)
                .GetAsync()
                .ConfigureAwait(false);
            
            return query.Any();

            // var sql = $"SELECT * FROM {baseTable} WHERE {key} = '{value}'";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            // return data.Rows.Count > 0;
        }

        public async Task<bool> IsExistInCache(string key, string val)
        {
            var data = await ReadCacheAsync()
                .ConfigureAwait(false);
            var search = data.AsEnumerable()
                .Where(row => row.Field<string>(key) == val);
            if (!search.Any()) return false;

            var filtered = search.CopyToDataTable();
            Log.Information($"Media found in Caches: {filtered.ToJson()}");
            return true;
        }

        public async Task SaveAsync(Dictionary<string, object> data)
        {
//            var json = TextHelper.ToJson(data);
            Log.Information(data.ToJson());
            var insert = await new Query(baseTable)
                .ExecForMysql()
                .InsertAsync(data)
                .ConfigureAwait(false);
            
            // var insert = await _mySqlProvider.Insert(baseTable, data);
            Log.Information($"SaveFile: {insert}");
        }

        public async Task<DataTable> GetAllMedia()
        {
            var query = await new Query(baseTable)
                .ExecForMysql(true)
                .GetAsync()
                .ConfigureAwait(false);
            // var sql = $"SELECT * FROM {baseTable}";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            var data = query.ToJson().MapObject<DataTable>();
            return data;
        }

        public async Task UpdateCacheAsync()
        {
            var data = await GetAllMedia()
                .ConfigureAwait(false); 
            Log.Information($"Updating Media Filter caches to {fileJson}");

            await data.WriteCacheAsync(fileJson)
                .ConfigureAwait(false);
        }

        public async Task<DataTable> ReadCacheAsync()
        {
            var dataTable = await fileJson.ReadCacheAsync<DataTable>()
                .ConfigureAwait(false);
            return dataTable;
        }
    }
}