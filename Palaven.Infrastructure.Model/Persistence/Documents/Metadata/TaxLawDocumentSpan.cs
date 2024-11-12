using Newtonsoft.Json;

namespace Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

public class TaxLawDocumentSpan
{
    [JsonProperty(PropertyName = "index")]
    public int Index { get; set; }

    [JsonProperty(PropertyName = "length")]
    public int Length { get; set; }
}
