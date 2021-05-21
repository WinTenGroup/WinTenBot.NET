using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Serilog;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Utils.IO;
using WinTenDev.Zizi.Utils.Text;

namespace WinTenDev.Zizi.Services.Google
{
    public class FirestoreService
    {
        private const string ProjectId = "wintenbot";
        private FirestoreDb Db { get; set; }

        private void MakeClient()
        {
            var credPath = BotSettings.GoogleCloudCredentialsPath.SanitizeSlash();
            Log.Information("Create Firestore client, cred {CredPath}", credPath);
            var clientBuilder = new FirestoreDbBuilder()
            {
                CredentialsPath = credPath,
                ProjectId = ProjectId
            };

            var client = clientBuilder.Build();
            Db = client;
        }

        public void Create(string path, object data)
        {
            if (Db == null) MakeClient();

            // var a = FirestoreDb.Create(projectId);
            Log.Information("Adding data to {Path}", path);
            Log.Debug("Data: {V}", data.ToJson(true));

            var collection = Db.Collection(path);
            collection.AddAsync(data);
        }

        public async Task ListDocument()
        {
            var collectionReference = Db.Collection("");
            var docs = await collectionReference.GetSnapshotAsync();

            foreach (var documentSnapshot in docs)
            {
                Log.Information("SnapShot: {0}", documentSnapshot.ToJson(true));
            }
        }
    }
}