namespace Palaven.Application.Model.DatasetManagement;

public class QueryFineTuningDatasetRequest
{
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
}
