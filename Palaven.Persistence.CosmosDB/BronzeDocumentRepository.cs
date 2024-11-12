using Liara.Integrations.Azure;
using Microsoft.Azure.Cosmos;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Persistence.CosmosDB;

public class BronzeDocumentRepository : DocumentRepository<BronzeDocument>
{
    public BronzeDocumentRepository(CosmosClient client, CosmosDBContainerOptions containerOptions) 
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }

    public override async Task<BronzeDocument> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var queryText = $"SELECT * FROM c WHERE c.id = '{id}'";

        var documents = await base.GetAsync(queryText, continuationToken: null, cancellationToken);

        return documents.SingleOrDefault()!;
    }
}
