namespace Palaven.Model.PerformanceEvaluation;

public class EvaluationSessionRougescoreMetrics
{
    public Guid EvaluationSessionId { get; set; }
    public string EvaluationExercise { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public string RougeType { get; set; } = default!;
    public float Precision { get; set; }
    public float Recall { get; set; }
    public float F1 { get; set; }
}
