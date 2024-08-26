namespace Palaven.Model.PerformanceEvaluation.Commands;

public class CreateFineTuningDataset
{
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
}
