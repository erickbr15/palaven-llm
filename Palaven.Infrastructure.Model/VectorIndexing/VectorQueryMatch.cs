namespace Palaven.Infrastructure.Model.VectorIndexing;

public class VectorQueryMatch
{
    public string Id { get; set; } = default!;
    public IList<float> Values { get; set; } = new List<float>();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public double Score { get; set; }
}
