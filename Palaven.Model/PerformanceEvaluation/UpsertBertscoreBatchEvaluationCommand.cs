namespace Palaven.Model.PerformanceEvaluation;

public class UpsertBertscoreBatchEvaluationCommand
{
    public Guid SessionId { get; set; }
    public string EvaluationExercise { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }
}
