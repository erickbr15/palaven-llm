namespace Palaven.Infrastructure.Model.AI;

public class EmbeddingsVectorUpsertModel
{    
    public string Namespace { get; set; } = default!;
    public string Text { get; set; } = default!;
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
