using Liara.Integrations.Azure;
using Microsoft.Azure.Cosmos;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Persistence.CosmosDB;

public class GoldenDocumentRepository : DocumentRepository<GoldenDocument>
{
    public GoldenDocumentRepository(CosmosClient client, CosmosDBContainerOptions containerOptions)
        : base(client, containerOptions.DatabaseId, containerOptions.ContainerId)
    {
    }

    public override async Task<GoldenDocument> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var queryText = $"SELECT * FROM c WHERE c.id = '{id}'";

        var documents = await base.GetAsync(queryText, continuationToken: null, cancellationToken);

        return documents.SingleOrDefault()!;
    }
}
