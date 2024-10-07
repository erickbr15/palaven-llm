using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class SilverDocumentRepository : DocumentRepository<SilverDocument>
{
    public SilverDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}
