namespace Palaven.Application.Model.PerformanceMetrics;

public class UpsertBleuBatchEvaluationRequest
{
    public Guid SessionId { get; set; }
    public string EvaluationExercise { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public float? BleuScore { get; set; }
}
