using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.VideoIntelligence.V1;
using Google.Cloud.Vision.V1;
using Google.Protobuf;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.IO;
using Zizi.Bot.Models;
using Feature = Google.Cloud.VideoIntelligence.V1.Feature;

namespace Zizi.Bot.Tools.GoogleCloud
{
    public static class GoogleVision
    {
        private static ImageAnnotatorClient Client { get; set; }
        private static VideoIntelligenceServiceClient VideoIntelligenceService { get; set; }

        private static ImageAnnotatorClient MakeClient()
        {
            var credPath = BotSettings.GoogleCloudCredentialsPath.SanitizeSlash();
            Log.Information("Instantiates a client, cred {CredPath}", credPath);
            var clientBuilder = new ImageAnnotatorClientBuilder
            {
                CredentialsPath = credPath
            };

            var client = clientBuilder.Build();

            VideoIntelligenceService = new VideoIntelligenceServiceClientBuilder()
            {
                CredentialsPath = credPath
            }.Build();

            return client;
        }

        public static string ScanText(string filePath)
        {
            Log.Information("GoogleVision detect text {FilePath}", filePath);
            if (Client == null)
            {
                Client = MakeClient();
            }

            Log.Information("Load the image file into memory");
            var image = Image.FromFile(filePath);

            Log.Information("Performs text detection on the image file");
            var response = Client.DetectText(image);

            Log.Information("ResponseCount: {Count}", response.Count);
            if (response.Count != 0)
            {
                return response[0].Description.HtmlEncode();
            }

            Log.Information("Seem no string result.");
            return null;

            // PrintAnnotation(response);
        }

        public static SafeSearchAnnotation SafeSearch(string filePath)
        {
            Log.Information("Google SafeSearch file {FilePath}", filePath);
            Log.Debug("Loading file into memory");
            var image = Image.FromFile(filePath);

            Log.Debug("Perform SafeSearch detection");
            var response = Client.DetectSafeSearch(image);

            return response;
        }

        public static async Task VideoIntelligenceAsync(string filePath)
        {
            Log.Information("Starting Google VideoIntelligence");

            Log.Debug("Loading content");
            var fileBytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
            var protoBuff = ByteString.CopyFrom(fileBytes);
            
            Log.Debug("Creating request");
            var request = new AnnotateVideoRequest()
            {
                InputContent = protoBuff,
                Features = {Feature.TextDetection}
            };

            Log.Debug("Annotating video");
            var operation = await (await VideoIntelligenceService.AnnotateVideoAsync(request)
                    .ConfigureAwait(false)).PollUntilCompletedAsync()
                .ConfigureAwait(false);

            Log.Debug("OperationResult: {0}", operation.Result.ToJson(true));
            var annotationResult = operation.Result.AnnotationResults.First();
            var text = annotationResult.TextAnnotations.First();
        }

        private static void  PrintAnnotation(IReadOnlyList<EntityAnnotation> entityAnnotations)
        {
            foreach (var annotation in entityAnnotations)
            {
                // if (annotation.Description != null)
                Log.Information("Annotation {V}", annotation.ToJson(true));
                Log.Information("Desc {Score} - {Description}", annotation.Score, annotation.Description);
            }
        }
    }
}