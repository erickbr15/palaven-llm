using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Ingest.Commands;
using Palaven.Model.Ingest.Documents;
using Palaven.Model.Ingest.Documents.Golden;

namespace Palaven.Ingest.Services;

public class IngestTaxLawDocumentService : IIngestTaxLawDocumentService
{
    private readonly IDocumentRepository<TaxLawDocumentArticle> _articleDocumentRepository;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _goldenArticleDocumentRepository;
    private readonly ITraceableCommand<IngestLawDocumentModel, IngestLawDocumentTaskInfo> _startTaxLawIngestCommand;
    private readonly ITraceableCommand<ExtractLawDocumentPagesModel, IngestLawDocumentTaskInfo> _extractTaxLawDocumentPagesCommand;
    private readonly ITraceableCommand<CreateGoldenArticleDocumentModel, Guid> _createGoldenArticleDocumentCommand;    

    public IngestTaxLawDocumentService(
        IDocumentRepository<TaxLawDocumentArticle> articleDocumentRepository,
        IDocumentRepository<TaxLawDocumentGoldenArticle> goldenArticleDocumentRepository,
        ITraceableCommand<IngestLawDocumentModel, IngestLawDocumentTaskInfo> startTaxLawIngestCommand,
        ITraceableCommand<ExtractLawDocumentPagesModel, IngestLawDocumentTaskInfo> extractTaxLawDocumentPagesCommand,
        ITraceableCommand<CreateGoldenArticleDocumentModel, Guid> createGoldenArticleDocumentCommand)
    {
        _articleDocumentRepository = articleDocumentRepository ?? throw new ArgumentNullException(nameof(articleDocumentRepository));
        _goldenArticleDocumentRepository = goldenArticleDocumentRepository ?? throw new ArgumentNullException(nameof(goldenArticleDocumentRepository));

        _startTaxLawIngestCommand = startTaxLawIngestCommand ?? throw new ArgumentNullException(nameof(startTaxLawIngestCommand));
        _extractTaxLawDocumentPagesCommand = extractTaxLawDocumentPagesCommand ?? throw new ArgumentNullException(nameof(extractTaxLawDocumentPagesCommand));
        _createGoldenArticleDocumentCommand = createGoldenArticleDocumentCommand ?? throw new ArgumentNullException(nameof(createGoldenArticleDocumentCommand));
    }

    public async Task<IResult<IngestLawDocumentTaskInfo>> IngestTaxLawDocumentAsync(IngestLawDocumentModel model, CancellationToken cancellationToken)
    {
        var startIngestResult = await StartTaxLawDocumentIngestAsync(Guid.NewGuid(), model, cancellationToken);
        var extractPagesResult = await ExtractTaxLawDocumentPagesAsync(startIngestResult, cancellationToken);

        return extractPagesResult;
    }

    private Task<IResult<IngestLawDocumentTaskInfo>> StartTaxLawDocumentIngestAsync(Guid operationId, IngestLawDocumentModel model, CancellationToken cancellationToken)
    {
        return _startTaxLawIngestCommand.ExecuteAsync(operationId, model, cancellationToken);
    }

    private async Task<IResult<IngestLawDocumentTaskInfo>> ExtractTaxLawDocumentPagesAsync(IResult<IngestLawDocumentTaskInfo> startLawDocumentResult, CancellationToken cancellationToken)
    {
        if(startLawDocumentResult.AnyErrorsOrValidationFailures)
        {
            return await Task.FromResult(Result<IngestLawDocumentTaskInfo>.Fail(startLawDocumentResult.ValidationErrors, startLawDocumentResult.Errors));
        }
                
        var model = new ExtractLawDocumentPagesModel
        {
            OperationId = startLawDocumentResult.Value.TraceId
        };

        var extractPagesResult = await _extractTaxLawDocumentPagesCommand.ExecuteAsync(model.OperationId, model, cancellationToken);
        return extractPagesResult;
    }

    public async Task CreateGoldenDocumentsAsync(Guid traceId, Guid lawId, int chunkSize, CancellationToken cancellationToken)
    {        
        var queryGoldenArticle = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\"");
        var goldenArticles = await _goldenArticleDocumentRepository.GetAsync(queryGoldenArticle, continuationToken: null, queryRequestOptions: null, cancellationToken: cancellationToken);
        var existingGoldenArticleIds = string.Join(",", goldenArticles.Select(ga => $"'{ga.ArticleId}'"));

        var query = string.IsNullOrEmpty(existingGoldenArticleIds) ?
            new QueryDefinition($"SELECT TOP {chunkSize} * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\" and IS_NULL(c.Content) = false") :            
            new QueryDefinition($"SELECT TOP {chunkSize} * FROM c WHERE c.TraceId = \"{traceId}\" and c.LawId = \"{lawId}\" and IS_NULL(c.Content) = false and c.id NOT IN ({existingGoldenArticleIds})");

        var articles = await _articleDocumentRepository.GetAsync(query, continuationToken: null, queryRequestOptions: null, cancellationToken: cancellationToken);

        articles = articles.Where(a=> !string.IsNullOrWhiteSpace(a.Content) && a.Content.Length > 100).ToList();

        foreach (var article in articles)
        {
            var articleId = new Guid(article.Id);
            var result = await _createGoldenArticleDocumentCommand.ExecuteAsync(traceId, new CreateGoldenArticleDocumentModel { ArticleId = articleId }, cancellationToken);

            if (result.AnyErrorsOrValidationFailures)
            {
                System.Diagnostics.Debug.WriteLine($"Golden article created: {!result.AnyErrorsOrValidationFailures}.");
            }            
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
