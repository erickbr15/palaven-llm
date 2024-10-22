using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Chat;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Palaven.Model.Data.Documents;
using Palaven.Model.Data.Documents.Metadata;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class CreateSilverDocumentCommandHandler : ICommandHandler<CreateSilverDocumentCommand, EtlTaskDocument>
{
    private readonly IOpenAiServiceClient _openAiChatService;
    private readonly IDocumentRepository<EtlTaskDocument> _taskDocumentRepository;
    private readonly IDocumentRepository<BronzeDocument> _bronzeStageRepository;
    private readonly IDocumentRepository<SilverDocument> _silverStageRepository;

    public CreateSilverDocumentCommandHandler(IOpenAiServiceClient openAiChatService, IDocumentRepository<EtlTaskDocument> taskDocumentRepository,
        IDocumentRepository<BronzeDocument> bronzeStageRepository, 
        IDocumentRepository<SilverDocument> silverStageRepository)
    {
        _openAiChatService = openAiChatService ?? throw new ArgumentNullException(nameof(openAiChatService));
        _taskDocumentRepository = taskDocumentRepository ?? throw new ArgumentNullException(nameof(taskDocumentRepository));
        _bronzeStageRepository = bronzeStageRepository ?? throw new ArgumentNullException(nameof(bronzeStageRepository));
        _silverStageRepository = silverStageRepository ?? throw new ArgumentNullException(nameof(silverStageRepository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(CreateSilverDocumentCommand inputModel, CancellationToken cancellationToken)
    {
        if (inputModel == null)
        {
            return Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(inputModel)) });
        }

        var etlTask = await FetchLatestEtlTaskVersionAsync(inputModel.OperationId, cancellationToken);
        if (etlTask == null)
        {
            return Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new InvalidOperationException("Unable to fetch the latest ETL task document.") });
        }

        var isBronzeProcessCompleted = etlTask.Metadata.ContainsKey(EtlMetadataKeys.BronzeLayerCompleted) &&
                                       bool.TryParse(etlTask.Metadata[EtlMetadataKeys.BronzeLayerCompleted], out var bronzeLayerCompleted) &&
                                       bronzeLayerCompleted;
        if (!isBronzeProcessCompleted)
        {
            return Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new InvalidOperationException("The bronze layer process is not completed.") });
        }

        var existingSilverDocuments = await FetchExistingSilverDocumentsAsync(etlTask, cancellationToken);

        var processedParagraphIds = existingSilverDocuments.SelectMany(d => d.Paragraphs).Select(p => p.ParagraphId).ToList();

        var bronzeDocuments = await FetchBronzeDocumentsAsync(etlTask, cancellationToken);
        
        var relevantParagraphs = ExtractRelevantParagraphs(bronzeDocuments);
        relevantParagraphs = relevantParagraphs.Where(p => !processedParagraphIds.Contains(p.ParagraphId)).ToList();

        var articles = await ExtractArticlesAsync(etlTask.TenantId, relevantParagraphs, cancellationToken);

        var (successfulUpserts, failedUpserts) = await SaveSilverDocumentsAsync(articles, etlTask, bronzeDocuments[0].Metadata, cancellationToken);

        PopulateSilverDocumentMetadataAndDetails(etlTask, articles.Count, successfulUpserts, failedUpserts);

        var dbResponse = await _taskDocumentRepository.UpsertAsync(etlTask!, new PartitionKey(etlTask!.TenantId.ToString()),
            itemRequestOptions: null, cancellationToken: cancellationToken);

        if (dbResponse.StatusCode != HttpStatusCode.OK)
        {
            var result = Result<EtlTaskDocument>.Fail(new List<ValidationError>(),
                new List<Exception> { new InvalidOperationException($"Unable to update the ETL task document. Status code: {dbResponse.StatusCode}") });
            result.Value = etlTask!;
            return result;
        }

        return Result<EtlTaskDocument>.Success(etlTask);
    }

    private async Task<EtlTaskDocument?> FetchLatestEtlTaskVersionAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = \"{operationId}\"");

        var queryResults = await _taskDocumentRepository.GetAsync(
            query,
            continuationToken: null,
            queryRequestOptions: null,
            cancellationToken);

        return queryResults.SingleOrDefault();
    }

    private async Task<IList<BronzeDocument>> FetchBronzeDocumentsAsync(EtlTaskDocument etlTaskDocument, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.trace_id = \"{etlTaskDocument.Id}\"");

        var queryResults = await _bronzeStageRepository.GetAsync(query,
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(etlTaskDocument.TenantId.ToString()) },
            cancellationToken);

        return queryResults.ToList();
    }

    private async Task<IList<SilverDocument>> FetchExistingSilverDocumentsAsync(EtlTaskDocument etlTaskDocument, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition($"SELECT * FROM c WHERE c.trace_id = \"{etlTaskDocument.Id}\"");

        var queryResults = await _silverStageRepository.GetAsync(query,
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(etlTaskDocument.TenantId.ToString()) },
            cancellationToken);

        return queryResults.ToList();
    }

    private IList<TaxLawDocumentParagraph> ExtractRelevantParagraphs(IEnumerable<BronzeDocument> bronzeDocuments)
    {
        var relevantParagraphs = new List<TaxLawDocumentParagraph>();
        foreach (var bronzeDocument in bronzeDocuments)
        {
            relevantParagraphs.AddRange(ExtractRelevantParagraphs(bronzeDocument));
        }

        return relevantParagraphs;
    }

    private async Task<IList<SilverDocument>> ExtractArticlesAsync(Guid tenantId, IList<TaxLawDocumentParagraph> paragraphs, CancellationToken cancellationToken)
    {
        var queue = new Queue<TaxLawDocumentParagraph>(
            paragraphs.OrderBy(p => p.PageNumber)
            .ThenBy(p => p.Spans
            .FirstOrDefault()?.Index ?? 0));

        var articles = new List<SilverDocument>();
        var articleParagraphs = new List<TaxLawDocumentParagraph>();

        while (queue.Count > 0)
        {            
            
            TaxLawDocumentParagraph paragraph = null;
            articleParagraphs.Clear();

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

            var article = await ExtractArticleAsync(tenantId, articleParagraphs, cancellationToken);

            articles.Add(article);
        }        

        return articles;
    }

    private async Task<SilverDocument> ExtractArticleAsync(Guid tenantId, IEnumerable<TaxLawDocumentParagraph> paragraphs, CancellationToken cancellationToken)
    {
        var articleContent = string.Join(Environment.NewLine, paragraphs.Select(l=>l.Content.Trim()));

        var chatGptCallOptions = new ChatCompletionCreationModel
        {
            Model = "gpt-3.5-turbo",
            Temperature = 0.0m,
            User = tenantId.ToString(),
            ResponseFormat = new ResponseFormat {  Type = "json_object" }
        };

        var messages = new List<Message>
        {
            new()
            {
                Role = "system",
                Content = "You are a very capable AI assistant that will extract and format the article content for me."
            },
            new()
            {
                Role = "user",
                Content = Resources.ChatGptPromptTemplates.ExtractArticlePromptTemplate.Replace("{working_text}", articleContent)
            }
        };
        
        var chatGptResponse = await _openAiChatService.CreateChatCompletionAsync(messages, chatGptCallOptions, cancellationToken);
        var completionResult = new ExtractArticleChatGptCompletionResult { Success = false };
        try
        {
            completionResult = JsonConvert.DeserializeObject<ExtractArticleChatGptCompletionResult>(
                chatGptResponse!.Choices.FirstOrDefault()?.Message.Content ?? string.Empty);
        }
        catch
        {
            // ignored
        }

        var documentArticle = new SilverDocument
        {
            ArticleId = completionResult!.Article,
            ArticleContent = completionResult.Content            
        };

        (documentArticle.Paragraphs as List<TaxLawDocumentParagraph>)!.AddRange(paragraphs);

        return documentArticle;
    }    

    private async Task<(int SuccessfulUpserts, int FailedUpserts)> SaveSilverDocumentsAsync(IList<SilverDocument> silverDocuments, EtlTaskDocument etlTask, IDictionary<string, string> documentsMetadata, CancellationToken cancellationToken)
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

                documentsMetadata.ToList().ForEach(kvp => document.Metadata.Add(kvp.Key, kvp.Value));

                var dbResponse = await _silverStageRepository.UpsertAsync(document, new PartitionKey(document.TenantId.ToString()), itemRequestOptions: null,
                    cancellationToken: cancellationToken);

                if (dbResponse.StatusCode != HttpStatusCode.Created && dbResponse.StatusCode != HttpStatusCode.OK)
                {
                    failedUpserts++;
                }
                else
                {
                    successfulUpserts++;
                }
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
        document.Metadata.Add(EtlMetadataKeys.SilverLayerProcessed, true.ToString());
        document.Metadata.Add(EtlMetadataKeys.SilverLayerCompleted, (extractedArticleCount == ingestedArticleCount).ToString());
        document.Metadata.Add(EtlMetadataKeys.SilverLayerExtractedArticleCount, extractedArticleCount.ToString());
        document.Metadata.Add(EtlMetadataKeys.SilverLayerIngestedArticleCount, ingestedArticleCount.ToString());

        document.Details.Add($"{DateTime.UtcNow.ToString()}. Silver layer processed. Extracted articles: {extractedArticleCount}. Ingested articles: {ingestedArticleCount}. Ingest article errors: {ingestArticleErrorCount}");
    }

    private static List<TaxLawDocumentParagraph> ExtractRelevantParagraphs(BronzeDocument document)
    {
        var relevantParagraphs = new List<TaxLawDocumentParagraph>();

        var currentParagraphIndex = 0;

        if (document.PageNumber == 1)
        {
            while (!document.Paragraphs[currentParagraphIndex].Content.StartsWith("Artículo"))
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

        var headingStrings = Resources.Etl.TaxLawDocumentHeadingStrings.Split("|", StringSplitOptions.RemoveEmptyEntries);
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
