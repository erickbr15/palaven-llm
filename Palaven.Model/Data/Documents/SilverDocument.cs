using Newtonsoft.Json;
using Palaven.Model.Data.Documents.Metadata;

namespace Palaven.Model.Data.Documents;

/// <summary>
///     Represents the full ETL silver document
/// </summary>
public class SilverDocument : MedalDocument
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

    [JsonProperty(PropertyName = "metadata")]
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
