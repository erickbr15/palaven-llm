using Newtonsoft.Json;
using Palaven.Model.Data.Documents.Metadata;

namespace Palaven.Model.Data.Documents;

/// <summary>
///     Represents the full ETL bronze document
/// </summary>
public class BronzeDocument : MedalDocument
{    
    [JsonProperty(PropertyName = "page_number")]
    public int PageNumber { get; set; }

    [JsonProperty(PropertyName = "document_paragraphs")]
    public IList<TaxLawDocumentParagraph> Paragraphs { get; set; } = new List<TaxLawDocumentParagraph>();    

    [JsonProperty(PropertyName = "metadata")]
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();  
}
