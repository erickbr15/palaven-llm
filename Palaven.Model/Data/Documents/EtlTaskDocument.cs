using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents;

public class EtlTaskDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "trace_id")]
    public Guid TraceId { get; set; }

    [JsonProperty(PropertyName = "user_id")]
    public Guid UserId { get; set; }

    [JsonProperty(PropertyName = "document_schema")]
    public string DocumentSchema { get; set; } = default!;

    [JsonProperty(PropertyName = "task_completed")]
    public bool IsTaskCompleted { get; set; }

}
