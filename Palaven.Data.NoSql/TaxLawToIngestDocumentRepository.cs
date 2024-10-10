using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Data.Documents;

namespace Palaven.Data;

public class TaxLawToIngestDocumentRepository : DocumentRepository<StartTaxLawIngestTaskDocument>
{
    public TaxLawToIngestDocumentRepository(CosmosClient client, CosmosContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }
}