using System.Collections.Generic;
using System.IO;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using Serilog;
using WinTenDev.Zizi.Utils.IO;
using Zizi.Bot.Models.MachineLearning;

namespace WinTenDev.Zizi.Host.Tools
{
    public static class NlpUtil
    {
        private static readonly string ModelName = "Storage/MachineLearning/MachineLearning.NLP.zip".EnsureDirectory();

        public static string[] Predict(string text)
        {
            if (!File.Exists(ModelName)) TrainModel();

            var context = new MLContext();

            DataViewSchema pipelineSchema;
            var pipeline = context.Model.Load(ModelName, out pipelineSchema);
            var engine = context.Model.CreatePredictionEngine<TextData, TextTokens>(pipeline);
            var predict = engine.Predict(new TextData() {Text = text}).Tokens;

            return predict;
        }

        public static void TrainModel()
        {
            var context = new MLContext();
            var emptyData = new List<TextData>();
            var data = context.Data.LoadFromEnumerable(emptyData);

            var tokenization = context.Transforms.Text.TokenizeIntoWords("Tokens", "Text",
                    separators: new[] {' ', '.', ',', '?', '!'})
                .Append(context.Transforms.Text.RemoveDefaultStopWords("Tokens", "Tokens",
                    StopWordsRemovingEstimator.Language.English));

            var transformer = tokenization.Fit(data);

            context.SaveModel(transformer, data);
        }

        public static void SaveModel(this MLContext context, ITransformer transformer, IDataView data)
        {
            Log.Information("Saving NLP Model to {0}", ModelName);
            if (!File.Exists(ModelName))
            {
                context.Model.Save(transformer, data.Schema, ModelName);
                Log.Information("NLP Model saved!");
            }
            else
            {
                Log.Information("File {0} is exist. Skip for now", ModelName);
            }
        }
    }
}