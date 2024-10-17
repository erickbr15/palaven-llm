using Newtonsoft.Json;
using Palaven.Model.Data.Documents.Metadata;

namespace Palaven.Model.Data.Documents;

public class GoldenDocument : MedalDocument
{
    /// <summary>
    ///     Article law id. Example: Artículo 1
    /// </summary>
    [JsonProperty(PropertyName = "article_id")]
    public string ArticleId { get; set; } = default!;

    [JsonProperty(PropertyName = "article_content")]
    public string ArticleContent { get; set; } = default!;

    [JsonProperty(PropertyName = "document_lines")]
    public IList<TaxLawDocumentParagraph> Lines { get; set; } = new List<TaxLawDocumentParagraph>();

    [JsonProperty(PropertyName = "instructions")]
    public IList<Instruction> Instructions { get; set; } = new List<Instruction>();    
}
