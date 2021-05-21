using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Serilog;
using WinTenDev.Zizi.Models.Configs;
using WinTenDev.Zizi.Utils.IO;

namespace WinTenDev.Zizi.Services.Google
{
    public class GDriveService
    {
        private readonly string[] Scopes = {DriveService.Scope.Drive};
        private const string AppName = "Zizi Uploader";

        public DriveService AuthDrive(GoogleCloudConfig cloudConfig)
        {
            Log.Information("Initializing GoogleDrive client");
            var googleCred = "BotSettings.GoogleDriveAuth".SanitizeSlash();

            Log.Debug("GoogleDrive cred {0}", googleCred);
            using var stream = new FileStream(googleCred, FileMode.Open, FileAccess.Read);

            Log.Debug("Authorizing client..");
            var credPath = Path.Combine("Storage", "Common", "gdrive-auth-token-store").SanitizeSlash()
                .EnsureDirectory();
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credPath, true)).Result;

            Log.Debug("Credential saved to {0}", credPath);

            Log.Debug("Initializing Drive service");
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppName
            });

            Log.Information("Creating GoogleDrive client finish");

            return service;
        }
    }
}