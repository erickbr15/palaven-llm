using Newtonsoft.Json;

namespace Palaven.Model.Ingest.Documents.Golden;

public class TaxLawDocumentGoldenArticle
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = default!;

    [JsonProperty(PropertyName = "tenantId")]
    public string TenantId { get; set; } = default!;
    public Guid TraceId { get; set; }
    public Guid LawId { get; set; }
    public string LawName { get; set; } = default!;
    public string LawAcronym { get; set; } = default!;
    public int LawYear { get; set; }
    public Guid ArticleId { get; set; }
    public string Article { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string LawDocumentVersion { get; set; } = default!;
    public TaxLawAugmentationData AugmentationData { get; set; } = new TaxLawAugmentationData();
    public IList<FineTuningInstruction> FineTuningInstructions { get; set; } = new List<FineTuningInstruction>();
    public string DocumentType { get; set; } = default!;
}
