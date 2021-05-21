using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Iam.v1;
using Google.Apis.Iam.v1.Data;
using Google.Apis.Services;
using Serilog;
using WinTenDev.Zizi.Utils;

namespace WinTenDev.Zizi.Services.Google
{
    public class ServiceAccountService
    {
        public ServiceAccountService()
        {

        }

        public void GenerateServiceAccount(int numberOfAccount = 1)
        {
            string[] scopes = new string[] {DriveService.Scope.Drive, IamService.Scope.CloudPlatform};
            var driveAuth = @"Storage/Common/ZiziBot-e27a736469e3.json";

            using var stream = new FileStream(driveAuth, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(scopes);

            var service = new IamService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            for (int i = 0; i < numberOfAccount; i++)
            {
                var uniqueId = StringUtil.GenerateUniqueId();

                var request = new CreateServiceAccountRequest()
                {
                    AccountId = $"fulan-{uniqueId}",
                    ServiceAccount = new ServiceAccount()
                    {
                        DisplayName = $"fulan {uniqueId}"
                    }
                };

                var serviceAccount = service.Projects.ServiceAccounts.Create(request, "projects/zizibot-295007").Execute();

                var name = serviceAccount.Name;
                var accountKey = service.Projects.ServiceAccounts.Keys.Create(new CreateServiceAccountKeyRequest()
                {
                    PrivateKeyType = "TYPE_GOOGLE_CREDENTIALS_FILE",
                    KeyAlgorithm = "KEY_ALG_RSA_2048"
                }, serviceAccount.Name).Execute();


                Log.Information("created: {0}", serviceAccount.Name);
            }
        }

        public IList<ServiceAccount> ListServiceAccounts(string projectId)
        {
            var driveAuth = @"Storage/Common/ZiziBot-e27a736469e3.json";
            string[] scopes = new string[] {DriveService.Scope.Drive, IamService.Scope.CloudPlatform};

            using var stream = new FileStream(driveAuth, FileMode.Open, FileAccess.Read);
            var credential = GoogleCredential.FromStream(stream)
                .CreateScoped(scopes);

            var service = new IamService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });

            var response = service.Projects.ServiceAccounts.List(
                "projects/" + projectId).Execute();
            foreach (ServiceAccount account in response.Accounts)
            {
                Console.WriteLine("Name: " + account.Name);
                Console.WriteLine("Display Name: " + account.DisplayName);
                Console.WriteLine("Email: " + account.Email);
                Console.WriteLine();
            }

            return response.Accounts;
        }


        public DriveService AuthServiceAccount()
        {
            string[] scopes = new string[] {DriveService.Scope.Drive};

            var jsonKey = "key.json";

            var stream = new FileStream(jsonKey, FileMode.Open, FileAccess.ReadWrite);
            var cred = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            //
            // var service = new DriveService(new BaseClientService.Initializer()
            // {
            //     HttpClientInitializer = cred,
            //     ApplicationName = "ANu"
            // });


            var email = "test-417@elated-graph-261115.iam.gserviceaccount.com";
            var credential = new ServiceAccountCredential(new ServiceAccountCredential.Initializer(email)
            {
                Scopes = scopes,
                User = "mail@gmail.com"
            });

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "DriveAPI",
            });


            return service;
        }
    }
}