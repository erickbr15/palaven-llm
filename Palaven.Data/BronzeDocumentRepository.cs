using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class BronzeDocumentRepository : DocumentRepository<BronzeDocument>
{
    public BronzeDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions) 
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}
