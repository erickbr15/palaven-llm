using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;

namespace Palaven.Data;

public class TaxLawIngestTaskDocumentRepository : DocumentRepository<TaxLawIngestTaskDocument>
{
    public TaxLawIngestTaskDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions) 
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}
