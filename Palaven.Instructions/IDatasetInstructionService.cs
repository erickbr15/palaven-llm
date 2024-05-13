namespace Palaven.Instructions;

public interface IDatasetInstructionService
{
    Task CreateInstructionDatasetAsync(Guid traceId, CancellationToken cancellationToken);
}
