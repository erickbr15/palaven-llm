using Newtonsoft.Json;
using Palaven.Model.Documents.Metadata;

namespace Palaven.Model.Documents;

/// <summary>
///     Represents the full ETL bronze document
/// </summary>
public class BronzeDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "tenantId")]
    public string TenantId { get; set; } = default!;

    [JsonProperty(PropertyName = "trace_id")]
    public Guid TraceId { get; set; }

    [JsonProperty(PropertyName = "law_id")]
    public Guid LawId { get; set; }

    [JsonProperty(PropertyName = "law_document_version")]
    public string LawDocumentVersion { get; set; } = default!;

    [JsonProperty(PropertyName = "page_number")]
    public int PageNumber { get; set; }

    [JsonProperty(PropertyName = "lines")]
    public IList<TaxLawDocumentLine> Lines { get; set; } = new List<TaxLawDocumentLine>();

    [JsonProperty(PropertyName = "document_type")]
    public string DocumentType { get; set; } = default!;
}
