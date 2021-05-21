using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata.Execution;
using WinTenDev.Zizi.Utils;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services
{
    public class MediaFilterService
    {
        private readonly string baseTable = "media_filters";
        private readonly string fileJson = "media_filter.json";
        private readonly QueryService _queryService;

        public MediaFilterService(QueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<bool> IsExist(string key, string value)
        {
            var factory = _queryService.CreateMySqlConnection();
            var query = await factory.FromTable(baseTable)
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
            var factory = _queryService.CreateMySqlConnection();
            var insert = await factory.FromTable(baseTable)
                .InsertAsync(data);

            // var insert = await _mySqlProvider.Insert(baseTable, data);
            Log.Information("SaveFile: {Insert}", insert);
        }

        public async Task<DataTable> GetAllMedia()
        {
            var factory = _queryService.CreateMySqlConnection();
            var query = await factory.FromTable(baseTable)
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