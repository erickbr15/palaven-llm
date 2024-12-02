namespace Palaven.Infrastructure.Model.Messaging;

public class CreateInstructionDatasetMessage
{
    public string OperationId { get; set; } = default!;
    public string TenantId { get; set; } = default!;
    public string[] DocumentIds { get; set; } = default!;
}
