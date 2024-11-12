namespace Palaven.Application.Model.Ingest;

public class ArticlesCurationResult
{
    public Guid OperationId { get; set; }
    public IList<Guid> CuratedDocumentIds { get; set; } = new List<Guid>();    
    public IList<Guid> FailedDocumentIds { get; set; } = new List<Guid>();
    public int InvalidDocumentsCount { get; set; }
}
