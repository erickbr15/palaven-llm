namespace Palaven.Model.PerformanceEvaluation;

public class CreateEvaluationSessionModel
{
    public Guid DatasetId { get; set; }
    public int BatchSize { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string DeviceInfo { get; set; } = default!;
}
