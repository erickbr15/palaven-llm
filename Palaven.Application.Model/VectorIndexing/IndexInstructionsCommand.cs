namespace Palaven.Application.Model.VectorIndexing;

public class IndexInstructionsCommand
{
    public Guid OperationId { get; set; }
    public Guid[] DocumentIds { get; set; } = default!;
}
