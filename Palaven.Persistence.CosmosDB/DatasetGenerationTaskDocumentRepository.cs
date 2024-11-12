using Liara.Integrations.Azure;
using Microsoft.Azure.Cosmos;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Persistence.CosmosDB;

public class DatasetGenerationTaskDocumentRepository : DocumentRepository<DatasetGenerationTaskDocument>
{
    public DatasetGenerationTaskDocumentRepository(CosmosClient client, CosmosDBContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }

    public override Task<DatasetGenerationTaskDocument> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
