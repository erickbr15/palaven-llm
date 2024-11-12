using Liara.Persistence.Abstractions;
using Microsoft.Azure.Cosmos;

namespace Palaven.Persistence.CosmosDB;

public abstract class DocumentRepository<TDocument> : IDocumentRepository<TDocument> where TDocument : class
{
    private readonly Container _container;

    protected DocumentRepository(CosmosClient client, string databaseId, string containerId)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(databaseId);
        ArgumentNullException.ThrowIfNull(containerId);

        _container = client.GetContainer(databaseId, containerId);
    }

    public abstract Task<TDocument> GetByIdAsync(string id, CancellationToken cancellationToken);

    public async Task<TDocument> GetByIdAsync(string id, string partitionKeyValue, CancellationToken cancellationToken)
    {
        var dbResponse = await _container.ReadItemAsync<TDocument>(id, new PartitionKey(partitionKeyValue), requestOptions: null, cancellationToken);

        if(dbResponse.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null!;
        }
        else if(dbResponse.StatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to get document with id {id} from CosmosDB. Status code: {dbResponse.StatusCode}");
        }
        return dbResponse;
    }

    public virtual async Task<IEnumerable<TDocument>> GetAsync(string textQuery, string? continuationToken, CancellationToken cancellationToken)
    {
        var queryDefinition = new QueryDefinition(textQuery);
        
        var query = _container.GetItemQueryIterator<TDocument>(queryDefinition, continuationToken, requestOptions: null);
        var results = new List<TDocument>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync(cancellationToken);
            results.AddRange(response.ToList());
        }

        return results;
    }

    public virtual async Task CreateAsync(TDocument item, string partitionKeyValue, CancellationToken cancellationToken)
    {
        var dbResponse = await _container.CreateItemAsync(item, new PartitionKey(partitionKeyValue), requestOptions: null, cancellationToken);        
        
        if(dbResponse.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Failed to create document in CosmosDB. Status code: {dbResponse.StatusCode}");
        }
    }

    public async Task UpsertAsync(TDocument item, string partitionKeyValue, CancellationToken cancellationToken)
    {
        var dbResponse = await _container.UpsertItemAsync(item, new PartitionKey(partitionKeyValue), requestOptions: null, cancellationToken);

        if(dbResponse.StatusCode != System.Net.HttpStatusCode.OK && dbResponse.StatusCode != System.Net.HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Failed to upsert document in CosmosDB. Status code: {dbResponse.StatusCode}");
        }
    }

    public async Task DeleteAsync(string id, string partitionKeyValue, CancellationToken cancellationToken)
    {
        var dbResponse = await _container.DeleteItemAsync<TDocument>(id, new PartitionKey(partitionKeyValue), requestOptions: null, cancellationToken);

        if(dbResponse.StatusCode != System.Net.HttpStatusCode.NoContent)
        {
            throw new InvalidOperationException($"Failed to delete document in CosmosDB. Status code: {dbResponse.StatusCode}");
        }
    }
}