using System.Diagnostics;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Zizi.Core.Models.Settings;
using Zizi.Core.Utils;

namespace WinTenDev.Mirror.Extensions
{
    public static class GoogleCloudServiceCollectionExtension
    {
        public static IServiceCollection AddGoogleDrive(this IServiceCollection services)
        {
            Log.Information("Adding GoogleDrive Service client..");

            services.AddScoped(provider =>
            {
                string[] scopes = {DriveService.Scope.Drive};

                var stack = new StackFrame();
                var method = stack.GetMethod();

                Log.Information("Loading {0} on Add Google Drive service..", nameof(AppConfig));
                var appConfig = provider.GetService<AppConfig>();

                if (appConfig == null)
                    return null;

                var drive = appConfig.GoogleCloudConfig;

                var driveAuth = drive.DriveAuth;

                if (!File.Exists(driveAuth))
                {
                    Log.Warning("Drive auth json is missing. Drive upload will be disabled.");
                    return null;
                }


                Log.Debug("GoogleDrive cred {0}", driveAuth);
                using var stream = new FileStream(driveAuth, FileMode.Open, FileAccess.Read);

                Log.Debug("Authorizing client..");
                var credPath = Path.Combine("Storage", "Common", "gdrive-auth-token-store").SanitizeSlash().EnsureDirectory();
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;

                Log.Debug("Credential saved to {0}", credPath);

                Log.Debug("Initializing Drive service");
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "ZiziBot"
                });


                return service;
            });

            Log.Information("Creating GoogleDrive service client finish.");

            return services;
        }
    }
}