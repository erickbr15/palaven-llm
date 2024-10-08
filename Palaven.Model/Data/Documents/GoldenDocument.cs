using Newtonsoft.Json;
using Palaven.Model.Data.Documents.Metadata;

namespace Palaven.Model.Data.Documents;

public class GoldenDocument
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "tenantId")]
    public string TenantId { get; set; } = default!;

    [JsonProperty(PropertyName = "trace_id")]
    public Guid TraceId { get; set; }

    [JsonProperty(PropertyName = "law_id")]
    public Guid LawId { get; set; }

    [JsonProperty(PropertyName = "law_name")]
    public string LawName { get; set; } = default!;

    [JsonProperty(PropertyName = "law_acronym")]
    public string LawAcronym { get; set; } = default!;

    [JsonProperty(PropertyName = "law_year")]
    public int LawYear { get; set; }

    [JsonProperty(PropertyName = "article_law_id")]
    public string ArticleLawId { get; set; } = default!;

    [JsonProperty(PropertyName = "article_content")]
    public string ArticleContent { get; set; } = default!;

    [JsonProperty(PropertyName = "article_lines")]
    public IList<TaxLawDocumentLine> ArticleLines { get; set; } = new List<TaxLawDocumentLine>();

    [JsonProperty(PropertyName = "instructions")]
    public IList<Instruction> Instructions { get; set; } = new List<Instruction>();

    [JsonProperty(PropertyName = "law_document_version")]
    public string LawDocumentVersion { get; set; } = default!;

    [JsonProperty(PropertyName = "document_type")]
    public string DocumentType { get; set; } = default!;
}
