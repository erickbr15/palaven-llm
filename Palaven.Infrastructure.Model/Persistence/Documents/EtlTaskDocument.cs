using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public class EtlTaskDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "tenant_id")]
    public Guid TenantId { get; set; }

    [JsonProperty(PropertyName = "user_id")]
    public Guid UserId { get; set; }

    [JsonProperty(PropertyName = "document_schema")]
    public string DocumentSchema { get; set; } = default!;

    [JsonProperty(PropertyName = "task_metadata")]
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    [JsonProperty(PropertyName = "task_details")]
    public IList<string> Details { get; set; } = new List<string>();

    [JsonProperty(PropertyName = "task_completed")]
    public bool IsTaskCompleted { get; set; }

    public string SerializeToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

}
