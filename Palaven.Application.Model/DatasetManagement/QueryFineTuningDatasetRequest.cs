namespace Palaven.Application.Model.DatasetManagement;

public class QueryFineTuningDatasetRequest
{
    public Guid SessionId { get; set; }
    public int? BatchNumber { get; set; }
}
