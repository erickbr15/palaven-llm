namespace Palaven.Application.Model.PerformanceMetrics;

public class BleuMetrics
{
    public Guid EvaluationSessionId { get; set; }
    public string EvaluationExercise { get; set; } = string.Empty;
    public int BatchNumber { get; set; }
    public float BleuScore { get; set; }        
}
