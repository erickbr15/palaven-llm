using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Liara.OpenAI.Model.Embeddings;

public class Embedding
{
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("embedding")]
    public JArray EmbeddingVector { get; set; } = default!;

    [JsonProperty("object")]
    public string Object { get; set; } = default!;

    public IList<double> ConvertToListOfDouble()
    {
        var result = new List<double>();
        
        if (EmbeddingVector != null)
        {
            result = EmbeddingVector.Select(x => x.Value<double>()).ToList();
        }

        return result;
    }
}
