namespace Palaven.Model.PerformanceEvaluation.Web;

public class ChatCompletionPerformanceEvaluationModel
{
    public int BatchNumber { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }
    public IList<RougePerformanceEvaluationModel> RougeScoreMetrics { get; set; } = new List<RougePerformanceEvaluationModel>();
}
