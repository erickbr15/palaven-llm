using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Data.Documents;
using Palaven.Model.Data.Entities;
using Palaven.Model.Datasets;
using Palaven.Model.PerformanceEvaluation;
using System.Data;

namespace Palaven.Core.Datasets;

public class InstructionDatasetService : IInstructionDatasetService
{
    private readonly IDocumentRepository<DatasetGenerationTaskDocument> _datasetGenerationTasksRepository;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleRepository;
    private readonly IDatasetsDataService _instructionDataService;
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public InstructionDatasetService(
        IDocumentRepository<DatasetGenerationTaskDocument> datasetGenerationTasksRepository,
        IDocumentRepository<GoldenDocument> goldenArticleRepository,
        IDatasetsDataService instructionDataService,
        IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _datasetGenerationTasksRepository = datasetGenerationTasksRepository ?? throw new ArgumentNullException(nameof(datasetGenerationTasksRepository));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
        _instructionDataService = instructionDataService ?? throw new ArgumentNullException(nameof(instructionDataService));
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task CreateInstructionDatasetAsync(CreateInstructionDataset model, CancellationToken cancellationToken)
    {
        if(model == null)
        {
            return;
        }
        
        var indexedGoldenArticleIds = await GetProcessedGoldenArticlesAsync(model.TraceId, cancellationToken);

        var tenantId = new Guid("69a03a54-4181-4d50-8274-d2d88ea911e4");

        var queryText = indexedGoldenArticleIds.Any() ?
            $"SELECT * FROM c WHERE c.TraceId = \"{model.TraceId}\" and c.id NOT IN ({string.Join(",", indexedGoldenArticleIds.Select(x => $"\"{x}\""))})" :
            $"SELECT * FROM c WHERE c.TraceId = \"{model.TraceId}\"";

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
                foreach (var instruction in goldenArticle.Instructions)
                {
                    var instructionEntity = new InstructionEntity
                    {
                        Instruction = instruction.InstructionText,
                        Response = instruction.Response,
                        Category = instruction.Type,
                        GoldenArticleId = new Guid(goldenArticle.Id),
                        LawId = goldenArticle.LawId,
                        //ArticleId = goldenArticle.ArticleLawId, TODO: Adjust the instruction entity to remove this property
                        DatasetId = model.DatasetId
                    };

                    await _instructionDataService.CreateAsync(instructionEntity, cancellationToken);
                }

                await _instructionDataService.SaveChangesAsync(cancellationToken);

                var datasetGenerationTaskDocument = new DatasetGenerationTaskDocument
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    TraceId = model.TraceId,
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

    public async Task<IResult<List<InstructionData>>> FetchInstructionsDatasetAsync(FetchInstructionsDataset model, CancellationToken cancellationToken)
    {
        var evaluationSession = await _performanceEvaluationDataService.GetEvaluationSessionAsync(model.SessionId, cancellationToken);
        if(evaluationSession == null)
        {
            return Result<List<InstructionData>>.Success(new List<InstructionData>());
        }

        var testInstructions = _instructionDataService.GetInstructionForTestingByEvaluationSession(model.SessionId, evaluationSession.BatchSize, model.BatchNumber);

        var instructionDataset = testInstructions
            .Select(i=> new InstructionData
            {
                InstructionId = i.Id,
                DatasetId = i.DatasetId,
                ChunckNumber = model.BatchNumber,
                Instruction = i.Instruction,
                Response = i.Response,
                Category = i.Category,
                GoldenArticleId = i.GoldenArticleId,
                LawId = i.LawId,
                ArticleId = i.ArticleId
            }).ToList();

        return Result<List<InstructionData>>.Success(instructionDataset);
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