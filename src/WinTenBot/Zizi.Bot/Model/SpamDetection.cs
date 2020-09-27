using Microsoft.ML.Data;

namespace Zizi.Bot.Model
{

    public class SpamInput
    {
        [LoadColumn(0)]
        public string Label { get; set; }
        [LoadColumn(1)]
        public string Message { get; set; }
    }

    public class SpamPrediction
    {
        [ColumnName("PredictedLabel")]
        public string IsSpam { get; set; }
        // public float Score { get; set; }
        // public float Probability { get; set; }
    }

    public class FromLabel
    {
        public string RawLabel { get; set; }
    }

    public class ToLabel
    {
        public bool Label { get; set; }
    }
}