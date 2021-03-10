using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Providers;

namespace Zizi.Bot.Tools
{
    public static class SyncUtil
    {
        public static async Task SyncRssHistoryToCloud()
        {
            var prevDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            var queryBase = new Query("rss_history");

            var localHistory = await queryBase
                .WhereLike("created_at", $"%{prevDate}%")
                .ExecForSqLite()
                .GetAsync()
                .ConfigureAwait(false);

            var mappedHistory = localHistory.ToJson().MapObject<List<RssHistory>>();
            Log.Information("RSS History {PrevDate} {Count}", prevDate, mappedHistory.Count);

            Log.Information("Migrating RSS History to Cloud");

            var valuesInsert = new List<string>();
            foreach (var history in mappedHistory)
            {
                var list1 = new List<object>();
                // list1.Add("aa","aaa");
                var values = $"('{history.Url}', '{history.RssSource}', '{history.ChatId}', '{history.Title}', " +
                             $"'{history.PublishDate}', '{history.Author}', '{history.CreatedAt}')";
                valuesInsert.Add(values);
                // var data = new Dictionary<string, object>()
                // {
                // {"url", history.Url},
                // {"rss_source", history.RssSource},
                // {"chat_id", history.ChatId},
                // {"title", history.Title},
                // {"publish_date", history.PublishDate},
                // {"author", history.Author},
                // {"created_at", history.CreatedAt}
                // };

                // await queryBase.ExecForMysql(true).InsertAsync(data);
            }

            var valuesStr = valuesInsert.MkJoin(", ");
            Log.Debug("RssHistory: \n{ValuesStr}", valuesStr);

            var sqlInsert = "INSERT INTO rss_history " +
                            "(url, rss_source, chat_id, title, publish_date, author, created_at) " +
                            $"VALUES {valuesInsert.MkJoin(", ")}";

            await sqlInsert.ExecForMysqlNonQueryAsync(printSql: true)
                .ConfigureAwait(false);

            // queryBase.ExecForMysql()

            Log.Information("RSS History migrated.");
        }

        public static async Task<int> SyncGBanToLocalAsync(bool cleanSync = false)
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Getting FBam data..");
            var cloudQuery = (await new Query("global_bans")
                    .ExecForMysql()
                    .GetAsync<GlobalBanData>()
                    .ConfigureAwait(false))
                .ToList();

            var mappedQuery = cloudQuery.ToJson(followProperty: true).MapObject<List<GlobalBanData>>();
            var rowCount = cloudQuery.Count;
            Log.Information("GBan User: {0} rows", rowCount);

            var jsonGBan = "gban-users".OpenJson();

            Log.Debug("Opening GBan collection");
            var gBanCollection = await jsonGBan.GetCollectionAsync<GlobalBanData>().ConfigureAwait(false);

            Log.Debug("Deleting old data");
            await gBanCollection.DeleteManyAsync(x => true).ConfigureAwait(false);

            Log.Debug("Inserting new data");
            await gBanCollection.InsertManyAsync(cloudQuery).ConfigureAwait(false);

            Log.Debug("GBanSync - Clearing Object..");
            jsonGBan.Dispose();
            // mappedQuery.Clear();

            Log.Debug("Complete sync GBan {0} items at {1}", rowCount, sw.Elapsed);

            sw.Stop();

            return rowCount;

            // var valuesBuilder = new List<string>();
            // foreach (var globalBan in mappedQuery)
            // {
            //     var values = new List<string>
            //     {
            //         $"'{globalBan.UserId}'",
            //         $"'{globalBan.ReasonBan.SqlEscape().RemoveThisChar("[]'")}'",
            //         $"'{globalBan.BannedBy}'",
            //         $"'{globalBan.BannedFrom}'",
            //         $"'{globalBan.CreatedAt}'"
            //     };
            //
            //     valuesBuilder.Add($"({values.MkJoin(", ")})");
            // }
            //
            // var insertCols = "(user_id,reason_ban,banned_by,banned_from,created_at)";
            // var insertVals = valuesBuilder;
            //
            // Log.Information("Values chunk by 1000 rows.");
            // var chunkInsert = insertVals.ChunkBy(1000);
            //
            // var step = 1;
            // foreach (var insert in chunkInsert)
            // {
            //     var values = insert.MkJoin(",\n");
            //     var insertSql = $"INSERT INTO fban_user {insertCols} VALUES \n{values}";
            //
            //     Log.Information($"Insert part {step++}");
            //     await insertSql.ExecForSqLite(true)
            //         .ConfigureAwait(false);
            // }

            // foreach (var globalBan in mappedQuery)
            // {
            //     var data = new Dictionary<string, object>();
            //     data.Add("user_id", globalBan.UserId);
            //     data.Add("reason_ban", globalBan.ReasonBan);
            //     data.Add("banned_by", globalBan.BannedBy);
            //     data.Add("banned_from", globalBan.BannedFrom);
            //     data.Add("created_at", globalBan.CreatedAt);
            //
            //     var insert = await new Query("fban_user")
            //         .ExecForSqLite(true)
            //         .InsertAsync(data);
            // }

            // await "fban_user".DeleteDuplicateRow("user_id")
            // .ConfigureAwait(false);
        }

        public static async Task SyncWordToLocalAsync()
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Starting Sync Words filter");
            var cloudQuery = (await new Query("word_filter")
                .ExecForMysql()
                .GetAsync()
                .ConfigureAwait(false)).ToList();

            var cloudWords = cloudQuery.ToJson().MapObject<List<WordFilter>>();

            var collection = LiteDbProvider.GetCollections<WordFilter>();
            collection.DeleteAll();
            collection.Insert(cloudWords);

            // var jsonWords = "local-words".OpenJson();

            // Log.Debug("Getting Words Collections");
            // var wordCollection = jsonWords.GetCollection<WordFilter>();

            // Log.Debug("Deleting old Words");
            // await wordCollection.DeleteManyAsync(x => x.Word != null)
            // .ConfigureAwait(false);

            // Log.Debug("Inserting new Words");
            // await wordCollection.InsertManyAsync(cloudWords)
            // .ConfigureAwait(false);

            Log.Information("Sync {0} Words complete in {1}", cloudWords.Count, sw.Elapsed);

            // jsonWords.Dispose();
            cloudQuery.Clear();
            cloudWords.Clear();
            sw.Stop();

            // var localQuery = (await new Query("word_filter")
            //     .ExecForSqLite()
            //     .GetAsync()
            //     .ConfigureAwait(false)).ToList();
            // var localWords = localQuery.ToJson().MapObject<List<WordFilter>>();
            //
            //
            // var diffWords = cloudWords
            //     .Where(c => localWords.All(l => l.Word != c.Word)).ToList();
            // Log.Debug($"DiffWords: {diffWords.Count} item(s)");
            //
            // if (diffWords.Count == 0)
            // {
            //     Log.Debug("Seem not need sync words to Local storage");
            //     return;
            // }
            //
            // Log.Information("Starting sync Words to Local");
            // var clearData = await new Query("word_filter")
            //     .ExecForSqLite(true)
            //     .DeleteAsync()
            //     .ConfigureAwait(false);
            //
            // Log.Information($"Deleting local Word Filter: {clearData} rows");
            //
            // foreach (var row in cloudWords)
            // {
            //     var data = new Dictionary<string, object>()
            //     {
            //         {"word", row.Word},
            //         {"is_global", row.IsGlobal},
            //         {"deep_filter", row.DeepFilter},
            //         {"from_id", row.FromId},
            //         {"chat_id", row.ChatId},
            //         {"created_at", row.CreatedAt}
            //     };
            //
            //     var insert = await new Query("word_filter")
            //         .ExecForSqLite()
            //         .InsertAsync(data)
            //         .ConfigureAwait(false);
            // }
            //
            // Log.Information($"Synced {cloudWords.Count} row(s)");
        }

        public static async Task SyncWordToLocalAsync(this QueryFactory factory)
        {
            var sw = Stopwatch.StartNew();
            Log.Information("Starting Sync Words filter");

            var wordFilters = (await factory.FromQuery(new Query("word_filter"))
                .GetAsync<WordFilter>()
                .ConfigureAwait(false)).ToList();

            // var cloudQuery = (await new Query("word_filter")
            // .ExecForMysql()
            // .GetAsync()
            // .ConfigureAwait(false)).ToList();

            // var cloudWords = cloudQuery.ToJson().MapObject<List<WordFilter>>();

            var collection = LiteDbProvider.GetCollections<WordFilter>();
            collection.DeleteAll();
            collection.Insert(wordFilters);

            // var jsonWords = "local-words".OpenJson();

            // Log.Debug("Getting Words Collections");
            // var wordCollection = jsonWords.GetCollection<WordFilter>();

            // Log.Debug("Deleting old Words");
            // await wordCollection.DeleteManyAsync(x => x.Word != null)
            // .ConfigureAwait(false);

            // Log.Debug("Inserting new Words");
            // await wordCollection.InsertManyAsync(cloudWords)
            // .ConfigureAwait(false);

            Log.Information("Sync {0} Words complete in {1}", wordFilters.Count, sw.Elapsed);

            // jsonWords.Dispose();
            // cloudQuery.Clear();
            // cloudWords.Clear();
            wordFilters.Clear();
            sw.Stop();
        }
    }
}