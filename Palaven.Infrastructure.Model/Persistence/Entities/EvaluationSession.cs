namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class EvaluationSession
{
    public Guid SessionId { get; set; }
    public Guid DatasetId { get; set; }
    public int BatchSize { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string DeviceInfo { get; set; } = default!;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
