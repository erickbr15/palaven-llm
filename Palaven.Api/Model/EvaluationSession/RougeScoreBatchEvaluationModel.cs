namespace Palaven.Api.Model.EvaluationSession
{
    public class RougeScoreBatchEvaluationModel
    {
        public string RougeScoreType { get; set; } = string.Empty;
        public int BatchNumber { get; set; }
        public float? Precision { get; set; }
        public float? Recall { get; set; }
        public float? F1 { get; set; }
    }
}
