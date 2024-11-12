using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public class DatasetGenerationTaskDocument
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("tenantId")]
    public Guid TenantId { get; set; }
    public Guid TraceId { get; set; }
    public Guid GoldenArticleId { get; set; }
    public string Task { get; set; } = default!;
    public DateTime StartedAt { get; set; }
    public DateTime? RestartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
}
