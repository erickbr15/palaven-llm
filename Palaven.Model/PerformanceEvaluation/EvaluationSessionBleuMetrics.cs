namespace Palaven.Model.PerformanceEvaluation;

public class EvaluationSessionBleuMetrics
{
    public Guid EvaluationSessionId { get; set; }
    public string EvaluationExercise { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public float BleuScore { get; set; }        
}
