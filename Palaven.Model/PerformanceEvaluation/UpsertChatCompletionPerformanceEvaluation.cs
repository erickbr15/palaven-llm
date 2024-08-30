namespace Palaven.Model.PerformanceEvaluation;

public class UpsertChatCompletionPerformanceEvaluation
{
    public Guid SessionId { get; set; }
    public int BatchNumber { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }
    public IList<RougePerformanceEvaluationModel> RougeScoreMetrics { get; set; } = new List<RougePerformanceEvaluationModel>();
}
