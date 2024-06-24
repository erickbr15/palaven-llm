namespace Palaven.Model.PerformanceEvaluation.Commands;

public class CreateEvaluationSessionModel
{        
    public Guid DatasetId { get; set; }
    public int BatchSize { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
}
