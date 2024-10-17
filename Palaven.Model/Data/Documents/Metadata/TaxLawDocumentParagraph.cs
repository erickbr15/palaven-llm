using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents.Metadata;

public sealed class TaxLawDocumentParagraph
{
    [JsonProperty(PropertyName = "parent_document_id")]
    public Guid DocumentId { get; set; }

    [JsonProperty(PropertyName = "paragraph_id")]
    public Guid ParagraphId { get; set; }

    [JsonProperty(PropertyName = "page_number")]
    public int PageNumber { get; set; }

    [JsonProperty(PropertyName = "spans")]
    public List<TaxLawDocumentSpan> Spans { get; set; } = new List<TaxLawDocumentSpan>();

    [JsonProperty(PropertyName = "content")]
    public string Content { get; set; } = default!;

    [JsonProperty(PropertyName = "bounding_boxes")]
    public List<System.Drawing.PointF> BoundingBoxes { get; set; } = new List<System.Drawing.PointF>();
}
