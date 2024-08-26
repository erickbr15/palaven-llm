namespace Palaven.Model.PerformanceEvaluation.Commands;

public class QueryFineTuningDataset
{
    public Guid SessionId { get; set; }
    public int? BatchNumber { get; set; }
}
