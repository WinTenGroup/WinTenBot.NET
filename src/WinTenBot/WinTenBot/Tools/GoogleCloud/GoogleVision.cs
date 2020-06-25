using System.Collections.Generic;
using Google.Cloud.Vision.V1;
using Serilog;
using WinTenBot.Common;
using WinTenBot.IO;
using WinTenBot.Model;

namespace WinTenBot.Tools.GoogleCloud
{
    public static class GoogleVision
    {
        private static ImageAnnotatorClient Client { get; set; }

        private static ImageAnnotatorClient MakeClient()
        {
            var credPath = BotSettings.GoogleCloudCredentialsPath.SanitizeSlash();
            Log.Information($"Instantiates a client, cred {credPath}");
            var clientBuilder = new ImageAnnotatorClientBuilder
            {
                CredentialsPath = credPath
            };

            var client = clientBuilder.Build();
            return client;
        }

        public static string ScanText(string filePath)
        {
            Log.Information($"GoogleVision detect text {filePath}");
            if (Client == null)
            {
                Client = MakeClient();
            }

            Log.Information("Load the image file into memory");
            var image = Image.FromFile(filePath);

            Log.Information("Performs text detection on the image file");
            var response = Client.DetectText(image);

            Log.Information($"ResponseCount: {response.Count}");
            if (response.Count != 0) return response[0].Description;

            Log.Information("Seem no string result.");
            return null;

            // PrintAnnotation(response);
        }

        public static SafeSearchAnnotation SafeSearch(string filePath)
        {
            Log.Information($"Google SafeSearch file {filePath}");
            Log.Debug("Loading file into memory");
            var image = Image.FromFile(filePath);
            
            Log.Debug("Perform SafeSearch detection");
            var response = Client.DetectSafeSearch(image);

            return response;
        }

        private static void PrintAnnotation(IReadOnlyList<EntityAnnotation> entityAnnotations)
        {
            foreach (var annotation in entityAnnotations)
            {
                // if (annotation.Description != null)
                Log.Information($"Annotation {annotation.ToJson(true)}");
                Log.Information($"Desc {annotation.Score} - {annotation.Description}");
            }
        }
    }
}