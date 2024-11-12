using Liara.Integrations.Azure;
using Microsoft.Azure.Cosmos;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Persistence.CosmosDB;

public class EtlTaskDocumentRepository : DocumentRepository<EtlTaskDocument>
{
    public EtlTaskDocumentRepository(CosmosClient client, CosmosDBContainerOptions containerOptions) 
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }

    public override async Task<EtlTaskDocument> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var querytext = $"SELECT * FROM c WHERE c.id = '{id}'";

        var documents = await base.GetAsync(querytext, continuationToken: null, cancellationToken);

        return documents.SingleOrDefault()!;
    }
}
