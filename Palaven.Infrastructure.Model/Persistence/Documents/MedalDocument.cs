using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public class MedalDocument
{
    [JsonProperty(PropertyName = "id")]
    public Guid Id { get; set; }

    [JsonProperty(PropertyName = "tenant_id")]
    public Guid TenantId { get; set; }

    [JsonProperty(PropertyName = "trace_id")]
    public Guid TraceId { get; set; }

    [JsonProperty(PropertyName = "document_schema")]
    public string DocumentSchema { get; set; } = default!;
}
