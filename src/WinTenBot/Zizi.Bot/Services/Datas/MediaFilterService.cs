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

namespace Zizi.Bot.Services.Datas
{
    public class MediaFilterService
    {
        private readonly string baseTable = "media_filters";
        private readonly string fileJson = "media_filter.json";

        public async Task<bool> IsExist(string key, string value)
        {
            var query = await new Query(baseTable)
                .ExecForMysql(true)
                .Where(key, value)
                .GetAsync();

            return query.Any();

            // var sql = $"SELECT * FROM {baseTable} WHERE {key} = '{value}'";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            // return data.Rows.Count > 0;
        }

        public async Task<bool> IsExistInCache(string key, string val)
        {
            var data = await ReadCacheAsync();
            var search = data.AsEnumerable()
                .Where(row => row.Field<string>(key) == val);
            if (!search.Any()) return false;

            var filtered = search.CopyToDataTable();
            Log.Information("Media found in Caches: {V}", filtered.ToJson());
            return true;
        }

        public async Task SaveAsync(Dictionary<string, object> data)
        {
            //            var json = TextHelper.ToJson(data);
            Log.Information("Data : {0}", data.ToJson(true));
            var insert = await new Query(baseTable)
                .ExecForMysql()
                .InsertAsync(data);

            // var insert = await _mySqlProvider.Insert(baseTable, data);
            Log.Information("SaveFile: {Insert}", insert);
        }

        public async Task<DataTable> GetAllMedia()
        {
            var query = await new Query(baseTable)
                .ExecForMysql(true)
                .GetAsync();
            // var sql = $"SELECT * FROM {baseTable}";
            // var data = await _mySqlProvider.ExecQueryAsync(sql);
            var data = query.ToJson().MapObject<DataTable>();
            return data;
        }

        public async Task UpdateCacheAsync()
        {
            var data = await GetAllMedia();
            Log.Information("Updating Media Filter caches to {FileJson}", fileJson);

            await data.WriteCacheAsync(fileJson);
        }

        public async Task<DataTable> ReadCacheAsync()
        {
            var dataTable = await fileJson.ReadCacheAsync<DataTable>();
            return dataTable;
        }
    }
}