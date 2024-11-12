namespace Palaven.Application.Model.VectorIndexing;

public class InstructionsIndexingResult
{
    public Guid OperationId { get; set; }
    public IList<Guid> SucessfulDocumentIds { get; set; } = new List<Guid>();
    public IList<Guid> FailedDocumentIds { get; set; } = new List<Guid>();
}
