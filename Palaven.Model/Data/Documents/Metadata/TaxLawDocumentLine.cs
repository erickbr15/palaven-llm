using Newtonsoft.Json;

namespace Palaven.Model.Data.Documents.Metadata;

public sealed class TaxLawDocumentLine
{
    [JsonProperty(PropertyName = "page_document_id")]
    public Guid PageDocumentId { get; set; }

    [JsonProperty(PropertyName = "line_id")]
    public Guid LineId { get; set; }

    [JsonProperty(PropertyName = "page_number")]
    public int PageNumber { get; set; }

    [JsonProperty(PropertyName = "line_number")]
    public int LineNumber { get; set; }

    [JsonProperty(PropertyName = "content")]
    public string Content { get; set; } = default!;

    [JsonProperty(PropertyName = "bounding_box")]
    public List<System.Drawing.PointF> BoundingBox { get; set; } = new List<System.Drawing.PointF>();
}
