namespace Palaven.Application.Model.Ingest;

public class EnqueueInstructionTransformationTasksCommand
{
    public Guid OperationId { get; set; }
    public Guid TenantId { get; set; }
    public int BatchSize { get; set; }
}
