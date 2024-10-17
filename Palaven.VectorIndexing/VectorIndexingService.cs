using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Data.Documents;
using Palaven.Model.VectorIndexing;

namespace Palaven.VectorIndexing;

public class VectorIndexingService : IVectorIndexingService
{
    private readonly IDocumentRepository<DatasetGenerationTaskDocument> _datasetGenerationTasksRepository;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleRepository;
    private readonly ICommandHandler<UploadGoldenArticleToVectorIndexCommand, Guid> _uploadGoldenArticleToVectorIndex;

    public VectorIndexingService(
        IDocumentRepository<DatasetGenerationTaskDocument> datasetGenerationTasksRepository,
        IDocumentRepository<GoldenDocument> goldenArticleRepository,
        ICommandHandler<UploadGoldenArticleToVectorIndexCommand, Guid> uploadGoldenArticleToVectorIndex)
    {
        _datasetGenerationTasksRepository = datasetGenerationTasksRepository ?? throw new ArgumentNullException(nameof(datasetGenerationTasksRepository));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
        _uploadGoldenArticleToVectorIndex = uploadGoldenArticleToVectorIndex ?? throw new ArgumentNullException(nameof(uploadGoldenArticleToVectorIndex));
    }

    public async Task CreateVectorIndexAsync(Guid traceId, CancellationToken cancellationToken)
    {
        var indexedGoldenArticleIds = await GetIndexedGoldenArticlesAsync(traceId, cancellationToken);                
        
        var tenantId = new Guid("69a03a54-4181-4d50-8274-d2d88ea911e4");
        
        var queryText = indexedGoldenArticleIds.Any() ?
            $"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.id NOT IN ({string.Join(",", indexedGoldenArticleIds.Select(x => $"\"{x}\""))})" :
            $"SELECT * FROM c WHERE c.TraceId = \"{traceId}\"";

        var query = new QueryDefinition(queryText);

        var goldenArticles = await _goldenArticleRepository.GetAsync(
            query, 
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);
        
        foreach (var goldenArticle in goldenArticles ?? new List<GoldenDocument>())
        {
            var indexCreationResult = await _uploadGoldenArticleToVectorIndex.ExecuteAsync(new UploadGoldenArticleToVectorIndexCommand { TraceId = traceId, GoldenArticleId = goldenArticle.Id }, cancellationToken);
            if (indexCreationResult.IsSuccess)
            {
                var datasetGenerationTaskDocument = new DatasetGenerationTaskDocument
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TraceId = traceId,
                    Task = "vector_index_creation",
                    GoldenArticleId = goldenArticle.Id,
                    StartedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                await _datasetGenerationTasksRepository.CreateAsync(
                    datasetGenerationTaskDocument, 
                    new PartitionKey(tenantId.ToString()), 
                    itemRequestOptions: null, 
                    cancellationToken);
            }
        }
    }

    private async Task<IList<Guid>> GetIndexedGoldenArticlesAsync(Guid traceId, CancellationToken cancellationToken)
    {        
        var tenantId = new Guid("69a03a54-4181-4d50-8274-d2d88ea911e4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.Task = \"vector_index_creation\" and IS_NULL(c.FinishedAt) = false");
        var queryResults = await _datasetGenerationTasksRepository.GetAsync(query, continuationToken: null, new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var indexedGoldenArticleIds = queryResults.Select(x => x.GoldenArticleId).ToList();

        return indexedGoldenArticleIds;
    }

}
