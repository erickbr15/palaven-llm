using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.VectorIndexing;

namespace Palaven.Infrastructure.Abstractions.VectorIndexing;

public interface IVectorIndexService
{
    Task UpsertAsync(IList<EmbeddingsVectorUpsertModel> vectorUpsertModels, CancellationToken cancellationToken);

    Task<IList<VectorQueryMatch>> QueryAsync(VectorQuery query, CancellationToken cancellationToken);
}
