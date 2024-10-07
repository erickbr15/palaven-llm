using Newtonsoft.Json;
using Palaven.Model.Documents.Metadata;

namespace Palaven.Model.Documents;

/// <summary>
///     Represents the full ETL silver document
/// </summary>
public class SilverDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "tenant_id")]
    public string TenantId { get; set; } = default!;

    [JsonProperty(PropertyName = "trace_id")]
    public Guid TraceId { get; set; }

    [JsonProperty(PropertyName = "law_id")]
    public Guid LawId { get; set; }

    [JsonProperty(PropertyName = "law_document_version")]
    public string LawDocumentVersion { get; set; } = default!;

    [JsonProperty(PropertyName = "article_law_id")]
    public string ArticleLawId { get; set; } = default!;

    [JsonProperty(PropertyName = "article_content")]
    public string ArticleContent { get; set; } = default!;

    [JsonProperty(PropertyName = "article_lines")]
    public IList<TaxLawDocumentLine> ArticleLines { get; set; } = new List<TaxLawDocumentLine>();

    [JsonProperty(PropertyName = "document_type")]
    public string DocumentType { get; set; } = default!;
}
