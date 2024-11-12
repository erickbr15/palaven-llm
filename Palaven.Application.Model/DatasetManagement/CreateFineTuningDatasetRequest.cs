namespace Palaven.Application.Model.DatasetManagement;

public class CreateFineTuningDatasetRequest
{
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
}
