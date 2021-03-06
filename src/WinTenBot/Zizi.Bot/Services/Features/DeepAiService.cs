using System.Threading.Tasks;
using Flurl.Http;
using Serilog;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Models.Settings;

namespace Zizi.Bot.Services.Features
{
    public class DeepAiService
    {
        private CommonConfig _commonConfig;

        // private readonly DeepAI_API _deepAiApi;

        public DeepAiService()
        {

        }

        public DeepAiService(CommonConfig commonConfig)
        {
            _commonConfig = commonConfig;
        }

        // public string NsfwDetector(string imagePath)
        // {
           // var resp = _deepAiApi.callStandardApi("nsfw-detector", new
            // {
                // image = imagePath
            // });

           // var json = _deepAiApi.objectAsJsonString(resp);

           // return json;
        // }

        public async Task<DeepAiResult> NsfwDetectCoreAsync(string imagePath)
        {
            var baseUrl = "https://api.deepai.org/api/nsfw-detector";
            var token = _commonConfig.DeepAiToken;

            var flurlResponse = await baseUrl
                .WithHeader("api-key",token)
                .PostMultipartAsync(content =>
            {
                content.AddFile("image", imagePath);
                // content.AddString("image", imagePath);
                // content.AddUrlEncoded("image", imagePath);
            });

            var deepAiResult = await flurlResponse.GetJsonAsync<DeepAiResult>();
            Log.Debug("NSFW Result: {0}", deepAiResult.ToJson(true));

            return deepAiResult;
        }
    }
}