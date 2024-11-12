namespace Palaven.Application.Model.Ingest;

public class GenerateInstructionsCommand
{
    public Guid OperationId { get; set; }
    public Guid[] DocumentIds { get; set; } = default!;
}
