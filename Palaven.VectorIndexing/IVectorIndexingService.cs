namespace Palaven.VectorIndexing;

public interface IVectorIndexingService
{
    Task CreateVectorIndexAsync(Guid traceId, CancellationToken cancellationToken);
}
