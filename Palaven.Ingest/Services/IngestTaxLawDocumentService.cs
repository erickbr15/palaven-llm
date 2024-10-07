using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;
using Palaven.Model.Ingest;

namespace Palaven.Ingest.Services;

public class IngestTaxLawDocumentService : IIngestTaxLawDocumentService
{
    private readonly IDocumentRepository<SilverDocument> _articleDocumentRepository;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleDocumentRepository;

    private readonly ICommandHandler<IngestTaxLawDocumentCommand, TaxLawDocumentIngestTask> _startTaxLawIngestCommand;
    private readonly ICommandHandler<CreateBronzeDocumentCommand, TaxLawDocumentIngestTask> _createBronzeDocumentCommandHandler;
    private readonly ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask> _createSilverDocumentCommandHandler;
    private readonly ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask> _createGoldenDocumentCommandHandler;    

    public IngestTaxLawDocumentService(
        IDocumentRepository<SilverDocument> articleDocumentRepository,
        IDocumentRepository<GoldenDocument> goldenArticleDocumentRepository,
        ICommandHandler<IngestTaxLawDocumentCommand, TaxLawDocumentIngestTask> startTaxLawIngestCommand,
        ICommandHandler<CreateBronzeDocumentCommand, TaxLawDocumentIngestTask> createBronzeDocumentCommandHandler,
        ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask> createSilverDocumentCommandHandler,
        ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask> createGoldenDocumentCommandHandler)
    {
        _articleDocumentRepository = articleDocumentRepository ?? throw new ArgumentNullException(nameof(articleDocumentRepository));
        _goldenArticleDocumentRepository = goldenArticleDocumentRepository ?? throw new ArgumentNullException(nameof(goldenArticleDocumentRepository));

        _startTaxLawIngestCommand = startTaxLawIngestCommand ?? throw new ArgumentNullException(nameof(startTaxLawIngestCommand));
        _createBronzeDocumentCommandHandler = createBronzeDocumentCommandHandler ?? throw new ArgumentNullException(nameof(createBronzeDocumentCommandHandler));
        _createSilverDocumentCommandHandler = createSilverDocumentCommandHandler ?? throw new ArgumentNullException(nameof(createSilverDocumentCommandHandler));
        _createGoldenDocumentCommandHandler = createGoldenDocumentCommandHandler ?? throw new ArgumentNullException(nameof(createGoldenDocumentCommandHandler));
    }

    public async Task<IResult<TaxLawDocumentIngestTask>> IngestTaxLawDocumentAsync(IngestTaxLawDocumentCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return await Task.FromResult(Result<TaxLawDocumentIngestTask>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command))}));
        }

        command.TraceId = Guid.NewGuid();

        var taskResult = await _startTaxLawIngestCommand.ExecuteAsync(command, cancellationToken);
        if (!taskResult.IsSuccess)
        {
            return await Task.FromResult(taskResult);
        }

        taskResult = await _createBronzeDocumentCommandHandler.ExecuteAsync(new CreateBronzeDocumentCommand { TraceId = command.TraceId }, cancellationToken);
        if(!taskResult.IsSuccess)
        {
            return await Task.FromResult(taskResult);
        }

        taskResult = await _createSilverDocumentCommandHandler.ExecuteAsync(new CreateSilverDocumentCommand { TraceId = command.TraceId }, cancellationToken);
        if (!taskResult.IsSuccess)
        {
            return await Task.FromResult(taskResult);
        }

        taskResult = await _createGoldenDocumentCommandHandler.ExecuteAsync(new CreateGoldenDocumentCommand { TraceId = command.TraceId }, cancellationToken);

        return taskResult;
    }        

    public async Task CreateGoldenDocumentBatchAsync(Guid traceId, Guid lawId, int batchSize, CancellationToken cancellationToken)
    {        
        var queryGoldenArticle = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\"");

        var goldenArticles = await _goldenArticleDocumentRepository.GetAsync(queryGoldenArticle, continuationToken: null, queryRequestOptions: null, cancellationToken: cancellationToken);

        var existingGoldenArticleIds = string.Join(",", goldenArticles.Select(ga => $"'{ga.Id}'"));

        var query = string.IsNullOrEmpty(existingGoldenArticleIds) ?
            new QueryDefinition($"SELECT TOP {batchSize} * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\" and IS_NULL(c.Content) = false") :            
            new QueryDefinition($"SELECT TOP {batchSize} * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\" and IS_NULL(c.Content) = false and c.id NOT IN ({existingGoldenArticleIds})");

        var articles = await _articleDocumentRepository.GetAsync(query, continuationToken: null, queryRequestOptions: null, cancellationToken: cancellationToken);

        articles = articles.Where(a=> !string.IsNullOrWhiteSpace(a.ArticleContent) && a.ArticleContent.Length > 100).ToList();

        foreach (var article in articles)
        {
            var articleId = new Guid(article.Id);
            await _createGoldenDocumentCommandHandler.ExecuteAsync(new CreateGoldenDocumentCommand { TraceId = traceId, ArticleId = articleId }, cancellationToken);
        }
    }

    public async Task DeleteGoldenDocumentsAsync(Guid traceId, Guid lawId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\"");
        var goldenArticles = await _goldenArticleDocumentRepository.GetAsync(query, continuationToken: null, queryRequestOptions: null, cancellationToken: cancellationToken);

        foreach (var goldenArticle in goldenArticles)
        {
            await _goldenArticleDocumentRepository.DeleteAsync(goldenArticle.Id, new PartitionKey(goldenArticle.TenantId), itemRequestOptions: null, cancellationToken);
        }
    }
}
