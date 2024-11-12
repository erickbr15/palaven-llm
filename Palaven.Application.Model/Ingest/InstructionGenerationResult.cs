namespace Palaven.Application.Model.Ingest;

public class InstructionGenerationResult
{
    public Guid OperationId { get; set; }
    public IList<Guid> SuccessfulDocumentIds { get; set; } = new List<Guid>();
    public IList<Guid> FailedSilverDocumentIds { get; set; } = new List<Guid>();
}
