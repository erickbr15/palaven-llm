using Liara.Integrations.Pinecone;
using Newtonsoft.Json.Linq;

namespace Palaven.Infrastructure.VectorIndexing;

public class PineconeVectorBuilder
{
    private Vector? _target;

    public PineconeVectorBuilder NewWith(JArray vector)
    {
        _target = new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = new List<double>(vector.Select(v => (double)v).ToArray())
        };

        return this;
    }

    public PineconeVectorBuilder NewWith(JArray vector, IDictionary<string, object> metadata)
    {
        _target = new Vector
        {
            Id = Guid.NewGuid().ToString(),
            Values = new List<double>(vector.Select(v => (double)v).ToArray())
        };

        if (metadata != null)
        {
            metadata.ToList().ForEach(m => _target.Metadata.Add(m.Key, m.Value));
        }

        return this;
    }

    public Vector Build()
    {
        return _target ?? throw new InvalidOperationException("Vector not initialized");
    }
}
