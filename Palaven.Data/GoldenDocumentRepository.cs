using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class GoldenDocumentRepository : DocumentRepository<GoldenDocument>
{
    public GoldenDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}
