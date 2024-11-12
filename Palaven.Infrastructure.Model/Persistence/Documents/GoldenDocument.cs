using Newtonsoft.Json;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

namespace Palaven.Infrastructure.Model.Persistence.Documents;

public class GoldenDocument : MedalDocument
{
    /// <summary>
    ///     Article law id. Example: Artículo 1
    /// </summary>
    [JsonProperty(PropertyName = "article_id")]
    public string ArticleId { get; set; } = default!;

    [JsonProperty(PropertyName = "article_content")]
    public string ArticleContent { get; set; } = default!;

    [JsonProperty(PropertyName = "article_paragraphs")]
    public IList<TaxLawDocumentParagraph> Paragraphs { get; set; } = new List<TaxLawDocumentParagraph>();

    [JsonProperty(PropertyName = "curated_by_ai")]
    public bool? CuratedByAIModel { get; set; }

    [JsonProperty(PropertyName = "curated_by_human_sme")]
    public bool? CuratedByHumanSME { get; set; }

    [JsonProperty(PropertyName = "metadata")]
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    [JsonProperty(PropertyName = "instructions")]
    public IList<Instruction> Instructions { get; set; } = new List<Instruction>();
}
