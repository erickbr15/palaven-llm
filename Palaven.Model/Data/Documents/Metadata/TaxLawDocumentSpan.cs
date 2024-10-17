using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents.Metadata;

public class TaxLawDocumentSpan
{
    [JsonProperty(PropertyName = "index")]
    public int Index { get; set; }

    [JsonProperty(PropertyName = "length")]
    public int Length { get; set; }
}
