using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Serilog;
using SqlKata;
using SqlKata.Execution;
using Zizi.Bot.Providers;
using Zizi.Bot.Common;
using Zizi.Bot.Models;
using Zizi.Bot.Services;

namespace Zizi.Bot.Tools
{
    public static class MachineLearning
    {
        private static readonly string tableName = "words_learning";
        private static MLContext mlContext;
        private static TransformerChain<ITransformer> model;
        public static PredictionEngine<SpamInput, SpamPrediction> SpamEngine { get; set; }

        public static void SetupEngine()
        {
            mlContext = new MLContext();
            var basePath = BotSettings.LearningDataSetPath;
            // var filePath = basePath + "SMSSpamCollection.csv";
            // var filePath = basePath + "SpamCollection.csv";

            Log.Information("Loading dataset.");
            // Specify the schema for spam data and read it into DataView.
            // var data = mlContext.Data.LoadFromTextFile<SpamInput>(filePath,
            // hasHeader: true, separatorChar: '\t',allowQuoting:true);

            var dataSet = new Query("words_learning")
                .SelectRaw("label Label, message Message")
                .ExecForMysql(true)
                .Get<LearnCsv>();

            Log.Debug("Load DataSet {V} row(s)", dataSet.Count());
            var data = mlContext.Data.LoadFromEnumerable(dataSet);

            Log.Information("Creating pipelines.");
            // Data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Conversion
                .MapValueToKey("Label", "Label")
                .Append(mlContext.Transforms.Text.FeaturizeText("FeaturesText",
                    new Microsoft.ML.Transforms.Text.TextFeaturizingEstimator.Options
                    {
                        WordFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options
                        { NgramLength = 2, UseAllLengths = true },
                        CharFeatureExtractor = new Microsoft.ML.Transforms.Text.WordBagEstimator.Options
                        { NgramLength = 3, UseAllLengths = false },
                    }, "Message"))
                .Append(mlContext.Transforms.CopyColumns("Features", "FeaturesText"))
                .Append(mlContext.Transforms.NormalizeLpNorm("Features", "Features"))
                .AppendCacheCheckpoint(mlContext);

            Log.Information("Set the training algorithm");
            var trainer = mlContext.MulticlassClassification
                .Trainers.OneVersusAll(mlContext.BinaryClassification
                    .Trainers.AveragedPerceptron(labelColumnName: "Label",
                        numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Label")
                .Append(mlContext.Transforms
                    .Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));
            var trainingPipeLine = dataProcessPipeline.Append(trainer);

            Log.Information("Evaluate the model using cross-validation.");
            // Cross-validation splits our dataset into 'folds', trains a model on some folds and
            // evaluates it on the remaining fold. We are using 5 folds so we get back 5 sets of scores.
            // Let's compute the average AUC, which should be between 0.5 and 1 (higher is better).
            Log.Information("=== Cross-validating to get model's accuracy metrics");
            var crossValidationResults =
                mlContext.MulticlassClassification.CrossValidate(data: data, estimator: trainingPipeLine,
                    numberOfFolds: 5);

            Log.Information("Trainer: {V}", trainer.ToJson(true));

            Log.Information("Starting train a model");
            // Now let's train a model on the full dataset to help us get better results
            var model = trainingPipeLine.Fit(data);

            Log.Information("Create a PredictionFunction from our model");
            var predictor = mlContext.Model.CreatePredictionEngine<SpamInput, SpamPrediction>(model);
            SpamEngine = predictor;

            Console.WriteLine("=============== Predictions for below data===============");
            // Test a few examples
            ClassifyMessage(predictor, "That's a great idea. It should work.");
            ClassifyMessage(predictor, "free medicine winner! congratulations");
            ClassifyMessage(predictor, "Yes we should meet over the weekend!");
            ClassifyMessage(predictor, "you win pills and free entry vouchers");
            ClassifyMessage(predictor, "Lorem ipsum dolor sit amet");
        }

        public static async Task SetupEngineAsync()
        {
            await Task.Run(() =>
            {
                Log.Information("Running async setup.");
                SetupEngine();
            });
        }

        public static bool PredictMessage(string message)
        {
            if (SpamEngine == null)
            {
                Log.Information("SpamEngine need be built.");
                SetupEngine();
            }

            var input = new SpamInput { Message = message };
            var predict = SpamEngine.Predict(input);
            var isSpam = predict.IsSpam == "spam";
            Log.Information("IsSpam: {IsSpam}", isSpam);

            return isSpam;
        }

        public static void ClassifyMessage(PredictionEngine<SpamInput, SpamPrediction> predictor, string message)
        {
            var input = new SpamInput { Message = message };
            var prediction = predictor.Predict(input);
            var isSpamStr = prediction.IsSpam == "spam" ? "spam" : "not spam";

            Log.Information("The message '{V}' is '{IsSpamStr}'", input.ToJson(true), isSpamStr);
        }

        public static void WriteToCsv()
        {
            var basePath = BotSettings.LearningDataSetPath;
            var filePath = basePath + "SpamCollection.csv";
            var data = new Query("words_learning")
                .SelectRaw("mark Mark, message Message")
                .ExecForMysql(true)
                .Get<LearnCsv>()
                .ToList();

            data.WriteRecords(filePath, delimiter: "\t");
        }

        public static async Task ImportCsv(this TelegramService telegramService, string filePath,
            string delimiter = ",",
            bool hasHeader = false)
        {
            var message = telegramService.Message;
            var chatId = message.Chat.Id;
            var fromId = message.From.Id;

            Log.Information("Loading file {FilePath}", filePath);

            var csvRecords = CsvUtil.ReadCsv<LearnCsv>(filePath, hasHeader: hasHeader, delimiter: delimiter);

            var values = csvRecords.Select(row =>
            {
                var label = row.Label;
                var msg = row.Message;
                var cols = new List<object> { label, msg, fromId, chatId };

                // return new { label, msg, fromId, chatId};
                return cols.AsEnumerable();
            });

            // new LearnData()
            // {
            // Label = row.Label,
            // Message = row.Message,
            // FromId = fromId,
            // ChatId = chatId
            // }

            Log.Information("Inserting {V} row(s).", values.Count());
            var chunkInsert = values.ChunkBy(1000);
            var cols = new[] { "label", "message", "from_id", "chat_id" };
            foreach (var value in chunkInsert)
            {
                var insert = new Query(tableName)
                    .ExecForMysql()
                    .Insert(cols, value);
                Log.Information("Inserted to {TableName} {Insert} row(s)", tableName, insert);
            }

            await tableName.MysqlDeleteDuplicateRowAsync("message", printSql: true);
        }
    }
}