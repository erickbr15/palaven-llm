using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class DatasetGenerationTaskDocumentRepository : DocumentRepository<DatasetGenerationTaskDocument>
{
    public DatasetGenerationTaskDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}
