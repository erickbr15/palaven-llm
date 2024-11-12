namespace Palaven.Application.Model.PerformanceEvaluation;

public class CreateEvaluationSessionCommand
{
    public Guid DatasetId { get; set; }
    public int BatchSize { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string DeviceInfo { get; set; } = default!;
    public decimal TrainingAndValidationSplit { get; set; }
}
