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
using System.Text.RegularExpressions;
using System.Threading;

namespace Palaven.Ingest.Commands;

public class CreateSilverDocumentCommandHandler : ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask>
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

    public async Task<IResult<TaxLawDocumentIngestTask>> ExecuteAsync(CreateSilverDocumentCommand inputModel, CancellationToken cancellationToken)
    {
        if (inputModel == null)
        {
            return Result<TaxLawDocumentIngestTask>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(inputModel)) });
        }

        var etlTask = await FetchLatestEtlTaskVersionAsync(inputModel.OperationId, cancellationToken);
        if (etlTask == null)
        {
            return Result<TaxLawDocumentIngestTask>.Fail(new List<ValidationError>(), new List<Exception> { new InvalidOperationException("Unable to fetch the latest ETL task document.") });
        }

        var isBronzeProcessCompleted = etlTask.Metadata.ContainsKey(EtlMetadataKeys.BronzeLayerCompleted) &&
                                       bool.TryParse(etlTask.Metadata[EtlMetadataKeys.BronzeLayerCompleted], out var bronzeLayerCompleted) &&
                                       bronzeLayerCompleted;
        if (!isBronzeProcessCompleted)
        {
            return Result<TaxLawDocumentIngestTask>.Fail(new List<ValidationError>(), new List<Exception> { new InvalidOperationException("The bronze layer process is not completed.") });
        }

        var bronzeDocuments = await FetchBronzeDocumentsAsync(etlTask, cancellationToken);

        /*
        var articles = await ExtractArticlesAsync(documentPages, cancellationToken);

        foreach (var article in articles)
        {                                            
            article.Id = Guid.NewGuid().ToString();
            article.TenantId = tenantId.ToString();
            article.TraceId = inputModel.TraceId;
            article.DocumentType = nameof(SilverDocument);            

            var result = await _silverStageRepository.CreateAsync(article, new PartitionKey(tenantId.ToString()), itemRequestOptions: null, cancellationToken);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException($"Unable to create the account file document. Status code: {result.StatusCode}");
            }
        }

        return Result<TaxLawDocumentIngestTask>.Success(new TaxLawDocumentIngestTask { TraceId = inputModel.TraceId });
        */
        throw new NotImplementedException();
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

    private async Task<IList<SilverDocument>> ExtractArticlesAsync(IList<BronzeDocument> pages, CancellationToken cancellationToken)
    {
        /*
        var documentLines = pages
            .SelectMany(p => p.Paragraphs)
            .OrderBy(p => p.PageNumber)
            .ThenBy(p => p.LineNumber);

        var documentMetadata = pages[0].Metadata;

        var linesProcessingQueue = new Queue<TaxLawDocumentParagraph>(documentLines);
        var workingArticleLineList = new List<TaxLawDocumentParagraph>();
        
        var articles = new List<SilverDocument>();

        while (linesProcessingQueue.Any())
        {
            var line = linesProcessingQueue.Dequeue();
            if (line.LineNumber == 1) 
            {
                do
                {
                    line = linesProcessingQueue.Dequeue();
                } while (linesProcessingQueue.Count > 0 && !string.Equals(line.Content, "Secretaría de Servicios Parlamentarios"));



                while (true)
                {
                    if (string.Equals(line.Content, "Secretaría de Servicios Parlamentarios"))
                    {
                        line = linesProcessingQueue.Dequeue();
                        break;
                    }
                    line = linesProcessingQueue.Dequeue();
                }                
            }

            string paginationTextPattern = @"\b\d+\sde\s\d+\b";
            bool isPaginationInfo = Regex.IsMatch(line.Content, paginationTextPattern);
            if(isPaginationInfo)
            {
                continue;
            }
            
            if (line.Content.StartsWith("Artículo") && !workingArticleLineList.Any())
            {
                workingArticleLineList.Add(line);
            }
            else if (line.Content.StartsWith("Artículo"))
            {
                var article = await ExtractArticleAsync(workingArticleLineList, cancellationToken);
                articles.Add(article);

                workingArticleLineList.Clear();
                workingArticleLineList.Add(line);                
            }
            else if (workingArticleLineList.Any())
            {
                workingArticleLineList.Add(line);
            }
        }

        return articles;
        */
        throw new NotImplementedException();
    }

    private async Task<SilverDocument> ExtractArticleAsync(IEnumerable<TaxLawDocumentParagraph> articleLines, CancellationToken cancellationToken)
    {
        var articleContent = string.Join(Environment.NewLine, articleLines.Select(l=>l.Content.Trim()));

        var chatGptCallOptions = new ChatCompletionCreationModel
        {
            Model = "gpt-3.5-turbo",
            Temperature = 0.0m,
            User = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4").ToString(),
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

        (documentArticle.Lines as List<TaxLawDocumentParagraph>)!.AddRange(articleLines);

        return documentArticle;
    }
}
