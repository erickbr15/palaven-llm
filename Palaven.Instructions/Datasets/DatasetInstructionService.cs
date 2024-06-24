using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Datasets;
using Palaven.Model.Ingest.Documents;
using Palaven.Model.Ingest.Documents.Golden;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.Datasets;

public class DatasetInstructionService : IDatasetInstructionService
{
    private readonly IDocumentRepository<DatasetGenerationTaskDocument> _datasetGenerationTasksRepository;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _goldenArticleRepository;
    private readonly IInstructionDataService _instructionDataService;

    public DatasetInstructionService(
        IDocumentRepository<DatasetGenerationTaskDocument> datasetGenerationTasksRepository,
        IDocumentRepository<TaxLawDocumentGoldenArticle> goldenArticleRepository,
        IInstructionDataService instructionDataService)
    {
        _datasetGenerationTasksRepository = datasetGenerationTasksRepository ?? throw new ArgumentNullException(nameof(datasetGenerationTasksRepository));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
        _instructionDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
    }

    public async Task CreateInstructionDatasetAsync(Guid traceId, CancellationToken cancellationToken)
    {
        var indexedGoldenArticleIds = await GetProcessedGoldenArticlesAsync(traceId, cancellationToken);

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

        foreach (var goldenArticle in goldenArticles)
        {
            try
            {
                foreach (var instruction in goldenArticle.FineTuningInstructions)
                {
                    var instructionEntity = new InstructionEntity
                    {
                        Instruction = instruction.Instruction,
                        Response = instruction.Response,
                        Category = instruction.Category,
                        GoldenArticleId = new Guid(goldenArticle.Id),
                        LawId = goldenArticle.LawId,
                        ArticleId = goldenArticle.ArticleId
                    };

                    await _instructionDataService.CreateAsync(instructionEntity, cancellationToken);
                }

                await _instructionDataService.SaveChangesAsync(cancellationToken);

                var datasetGenerationTaskDocument = new DatasetGenerationTaskDocument
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TraceId = traceId,
                    Task = "instruction_dataset_creation",
                    GoldenArticleId = new Guid(goldenArticle.Id),
                    StartedAt = DateTime.Now,
                    FinishedAt = DateTime.Now
                };

                await _datasetGenerationTasksRepository.CreateAsync(
                    datasetGenerationTaskDocument,
                    new PartitionKey(tenantId.ToString()),
                    itemRequestOptions: null,
                    cancellationToken);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Error creating instruction dataset");
            }
        }
    }

    public Task<IResult<List<InstructionData>>> FetchInstructionsDatasetAsync(QueryInstructionsDatasetModel model, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task<IList<Guid>> GetProcessedGoldenArticlesAsync(Guid traceId, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69a03a54-4181-4d50-8274-d2d88ea911e4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.Task = \"instruction_dataset_creation\" and IS_NULL(c.FinishedAt) = false");
        var queryResults = await _datasetGenerationTasksRepository.GetAsync(query, continuationToken: null, new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var indexedGoldenArticleIds = queryResults.Select(x => x.GoldenArticleId).ToList();

        return indexedGoldenArticleIds;
    }
}