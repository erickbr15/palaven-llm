namespace Palaven.Model.PerformanceEvaluation.Commands;

public class UpsertChatCompletionPerformanceEvaluationModel
{
    public Guid SessionId { get; set; }
    public int BatchNumber { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }
}
