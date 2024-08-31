namespace Palaven.Model.PerformanceEvaluation;

public class CreateFineTuningDataset
{
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
}
