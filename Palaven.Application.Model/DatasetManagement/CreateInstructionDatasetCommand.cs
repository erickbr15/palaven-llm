namespace Palaven.Application.Model.DatasetManagement;

public class CreateInstructionDatasetCommand
{
    public Guid OperationId { get; set; }
    public Guid[] DocumentIds { get; set; } = default!;
}
