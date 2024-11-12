using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Ingest.Resources;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Application.Model.Ingest;
using Liara.Common.Abstractions;
using Liara.Common;
using Liara.Persistence.Abstractions;

namespace Palaven.Application.Ingest.Commands;

public class ExtractArticleParagraphsCommandHandler : ICommandHandler<ExtractArticleParagraphsCommand, EtlTaskDocument>
{
    private readonly IDocumentRepository<EtlTaskDocument> _etlTaskRepository;    
    private readonly IDocumentRepository<BronzeDocument> _bronzeDocumentRepository;
    private readonly IDocumentRepository<SilverDocument> _silverDocumentRepository;

    public ExtractArticleParagraphsCommandHandler(IDocumentRepository<EtlTaskDocument> etlTaskRepository, 
        IDocumentRepository<BronzeDocument> bronzeDocumentRepository,
        IDocumentRepository<SilverDocument> silverDocumentRepository)
    {
        _etlTaskRepository = etlTaskRepository ?? throw new ArgumentNullException(nameof(etlTaskRepository));
        _bronzeDocumentRepository = bronzeDocumentRepository ?? throw new ArgumentNullException(nameof(bronzeDocumentRepository));
        _silverDocumentRepository = silverDocumentRepository ?? throw new ArgumentNullException(nameof(silverDocumentRepository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(ExtractArticleParagraphsCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return Result<EtlTaskDocument>.Fail(new ArgumentNullException(nameof(command)));
        }

        var etlTask = await _etlTaskRepository.GetByIdAsync(command.OperationId.ToString(), cancellationToken);        
        
        await PrepareSilverStageForProcessingAsync(etlTask, cancellationToken);

        var bronzeDocuments = await _bronzeDocumentRepository.GetAsync($"SELECT * FROM c WHERE c.trace_id = '{command.OperationId}'", 
            continuationToken: null, cancellationToken);

        var relevantParagraphs = ExtractRelevantParagraphs(bronzeDocuments);

        var silverDocuments = ExtractArticles(relevantParagraphs);

        var (successfulUpserts, failedUpserts) = await SaveSilverDocumentsAsync(silverDocuments, etlTask, cancellationToken);

        PopulateSilverDocumentMetadataAndDetails(etlTask, silverDocuments.Count, successfulUpserts, failedUpserts);

        await _etlTaskRepository.UpsertAsync(etlTask, etlTask.TenantId.ToString(), cancellationToken);

        return Result<EtlTaskDocument>.Success(etlTask);
    }

    private async Task PrepareSilverStageForProcessingAsync(EtlTaskDocument etlTask, CancellationToken cancellationToken)
    {
        if(!etlTask.Metadata.ContainsKey(EtlMetadataKeys.SilverLayerExtractionProcessed))
        {            
            return;
        }
        
        var existingSilverDocuments = await _silverDocumentRepository.GetAsync($"SELECT * FROM c WHERE c.trace_id = '{etlTask.Id}'", continuationToken: null, cancellationToken);
        foreach(var silverDocument in existingSilverDocuments)
        {
            await _silverDocumentRepository.DeleteAsync(silverDocument.Id.ToString(), silverDocument.TenantId.ToString(), cancellationToken);
        }
    }

    private IList<TaxLawDocumentParagraph> ExtractRelevantParagraphs(IEnumerable<BronzeDocument> bronzeDocuments)
    {
        var relevantParagraphs = new List<TaxLawDocumentParagraph>();
        foreach (var document in bronzeDocuments)
        {
            var paragraphs = ExtractRelevantParagraphs(document);
            relevantParagraphs.AddRange(paragraphs);
        }

        return relevantParagraphs;
    }

    private IList<SilverDocument> ExtractArticles(IList<TaxLawDocumentParagraph> paragraphs)
    {
        var queue = new Queue<TaxLawDocumentParagraph>(paragraphs
            .OrderBy(p => p.PageNumber)
            .ThenBy(p => p.Spans.FirstOrDefault()?.Index ?? 0));

        var articles = new List<SilverDocument>();

        List<TaxLawDocumentParagraph> articleParagraphs = null;

        while (queue.Count > 0)
        {
            TaxLawDocumentParagraph paragraph = null;
            articleParagraphs = new List<TaxLawDocumentParagraph>();

            do
            {
                paragraph = queue.Dequeue();
            } while (queue.Count > 0 && !paragraph.Content.Trim().StartsWith("Artículo"));

            if (paragraph != null)
            {
                articleParagraphs.Add(paragraph);
            }

            if (queue.Count > 0)
            {
                do
                {
                    paragraph = queue.Peek();
                    if (paragraph.Content.Trim().StartsWith("Artículo"))
                    {
                        break;
                    }

                    paragraph = queue.Dequeue();
                    articleParagraphs.Add(paragraph);

                } while (queue.Count > 0);
            }

            var article = new SilverDocument();            
            article.Paragraphs.AddRange(articleParagraphs);

            articles.Add(article);
        }

        return articles;
    }

    private async Task<(int SuccessfulUpserts, int FailedUpserts)> SaveSilverDocumentsAsync(IList<SilverDocument> silverDocuments, EtlTaskDocument etlTask, CancellationToken cancellationToken)
    {
        int successfulUpserts = 0;
        int failedUpserts = 0;

        foreach (var document in silverDocuments)
        {
            try
            {
                document.Id = Guid.NewGuid();
                document.TenantId = etlTask.TenantId;
                document.TraceId = new Guid(etlTask.Id);
                document.DocumentSchema = nameof(SilverDocument);

                etlTask.Metadata
                    .Where(kv => EtlMetadataKeys.DocumentMetadataKeys.Contains(kv.Key))
                    .ToList()
                    .ForEach(kv => document.Metadata.Add(kv.Key, kv.Value));                

                await _silverDocumentRepository.UpsertAsync(document, document.TenantId.ToString(), cancellationToken);

                successfulUpserts++;
            }
            catch
            {
                failedUpserts++;
            }
        }

        return (successfulUpserts, failedUpserts);
    }

    private void PopulateSilverDocumentMetadataAndDetails(EtlTaskDocument document, int extractedArticleCount, int ingestedArticleCount, int ingestArticleErrorCount)
    {
        document.Metadata.Remove(EtlMetadataKeys.SilverLayerExtractionProcessed);
        document.Metadata.Remove(EtlMetadataKeys.SilverLayerExtractionCompleted);
        document.Metadata.Remove(EtlMetadataKeys.SilverLayerExtractedArticleCount);
        document.Metadata.Remove(EtlMetadataKeys.SilverLayerIngestedArticleCount);

        document.Metadata.TryAdd(EtlMetadataKeys.SilverLayerExtractionProcessed, true.ToString());
        document.Metadata.TryAdd(EtlMetadataKeys.SilverLayerExtractionCompleted, (extractedArticleCount == ingestedArticleCount).ToString());
        document.Metadata.TryAdd(EtlMetadataKeys.SilverLayerExtractedArticleCount, extractedArticleCount.ToString());
        document.Metadata.TryAdd(EtlMetadataKeys.SilverLayerIngestedArticleCount, ingestedArticleCount.ToString());

        document.Details.Add($"{DateTime.UtcNow.ToString()}. Silver layer processed. Extracted articles: {extractedArticleCount}. Ingested articles: {ingestedArticleCount}. Ingest article errors: {ingestArticleErrorCount}");
    }

    private static List<TaxLawDocumentParagraph> ExtractRelevantParagraphs(BronzeDocument document)
    {
        var relevantParagraphs = new List<TaxLawDocumentParagraph>();

        var currentParagraphIndex = 0;

        if (document.PageNumber == 1)
        {
            while (currentParagraphIndex < document.Paragraphs.Count && 
                !(document.Paragraphs[currentParagraphIndex].Content ?? string.Empty).StartsWith("Artículo"))
            {
                currentParagraphIndex++;
            }

            while (currentParagraphIndex < document.Paragraphs.Count)
            {
                relevantParagraphs.Add(document.Paragraphs[currentParagraphIndex]);
                currentParagraphIndex++;
            }

            return relevantParagraphs;
        }

        var headingStrings = Etl.TaxLawDocumentHeadingStrings.Split("|", StringSplitOptions.RemoveEmptyEntries);
        while (currentParagraphIndex < document.Paragraphs.Count)
        {
            var paragraph = document.Paragraphs[currentParagraphIndex];
            if (!headingStrings.Any(h => paragraph.Content.Contains(h)))
            {
                relevantParagraphs.Add(paragraph);
            }
            currentParagraphIndex++;
        }

        return relevantParagraphs;
    }
}
