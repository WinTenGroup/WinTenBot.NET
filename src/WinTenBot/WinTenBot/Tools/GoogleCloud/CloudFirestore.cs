using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Serilog;
using WinTenBot.Common;
using WinTenBot.IO;
using WinTenBot.Model;

namespace WinTenBot.Tools.GoogleCloud
{
    public static class CloudFirestore
    {
        private const string ProjectId = "wintenbot";
        private static FirestoreDb Db { get; set; }

        private static void MakeClient()
        {
            var credPath = BotSettings.GoogleCloudCredentialsPath.SanitizeSlash();
            Log.Information($"Create Firestore client, cred {credPath}");
            var clientBuilder = new FirestoreDbBuilder()
            {
                CredentialsPath = credPath,
                ProjectId = ProjectId
            };

            var client = clientBuilder.Build();
            Db = client;
        }

        public static void Create(string path, object data)
        {
            if (Db == null) MakeClient();
            
            // var a = FirestoreDb.Create(projectId);
            Log.Information($"Adding data to {path}");
            Log.Debug($"Data: {data.ToJson(true)}");

            var collection = Db.Collection(path);
            collection.AddAsync(data);
        }

        public static async Task ListDocument()
        {
            var collectionReference = Db.Collection("");
            var docs = await collectionReference.GetSnapshotAsync();

            foreach (var VARIABLE in docs)
            {
                Logger.LogInfo($"{VARIABLE.ToJson(true)}");
            }
        }
    }
}