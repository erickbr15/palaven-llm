using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public class NotificationDocument
{
    [JsonProperty(PropertyName = "id")]
    public Guid Id { get; set; }

    [JsonProperty(PropertyName = "tenant_id")]
    public Guid TenantId { get; set; }    

    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; } = default!;

    [JsonProperty(PropertyName = "created_at")]
    public DateTime CreatedAt { get; set; }        
}
