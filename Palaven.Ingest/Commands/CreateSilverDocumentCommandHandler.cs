using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Chat;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Palaven.Model.Documents;
using Palaven.Model.Documents.Metadata;
using Palaven.Model.Ingest;
using System.Net;
using System.Text.RegularExpressions;

namespace Palaven.Ingest.Commands;

public class CreateSilverDocumentCommandHandler : ICommandHandler<CreateSilverDocumentCommand, TaxLawDocumentIngestTask>
{
    private readonly IOpenAiServiceClient _openAiChatService;
    private readonly IDocumentRepository<BronzeDocument> _lawPageDocumentRepository;
    private readonly IDocumentRepository<SilverDocument> _articleDocumentRepository;

    public CreateSilverDocumentCommandHandler(IOpenAiServiceClient openAiChatService,
        IDocumentRepository<BronzeDocument> lawPageDocumentRepository, 
        IDocumentRepository<SilverDocument> articleDocumentRepository)
    {
        _openAiChatService = openAiChatService ?? throw new ArgumentNullException(nameof(openAiChatService));
        _lawPageDocumentRepository = lawPageDocumentRepository ?? throw new ArgumentNullException(nameof(lawPageDocumentRepository));
        _articleDocumentRepository = articleDocumentRepository ?? throw new ArgumentNullException(nameof(articleDocumentRepository));
    }

    public async Task<IResult<TaxLawDocumentIngestTask>> ExecuteAsync(CreateSilverDocumentCommand inputModel, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.TraceId = \"{inputModel.TraceId}\"");

        var queryResults = await _lawPageDocumentRepository.GetAsync(query, continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var documentPages = queryResults.ToList();
        var articles = await ExtractArticlesAsync(documentPages, cancellationToken);

        foreach (var article in articles)
        {                                            
            article.Id = Guid.NewGuid().ToString();
            article.TenantId = tenantId.ToString();
            article.TraceId = inputModel.TraceId;
            article.DocumentType = nameof(SilverDocument);
            article.LawDocumentVersion = documentPages.FirstOrDefault()?.LawDocumentVersion ?? string.Empty;
            article.LawId = documentPages.FirstOrDefault()?.LawId ?? Guid.Empty;

            var result = await _articleDocumentRepository.CreateAsync(article, new PartitionKey(tenantId.ToString()), itemRequestOptions: null, cancellationToken);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException($"Unable to create the account file document. Status code: {result.StatusCode}");
            }
        }

        return Result<TaxLawDocumentIngestTask>.Success(new TaxLawDocumentIngestTask { TraceId = inputModel.TraceId });
    }

    private async Task<IList<SilverDocument>> ExtractArticlesAsync(IList<BronzeDocument> pages, CancellationToken cancellationToken)
    {
        var lines = new Queue<TaxLawDocumentLine>(pages.SelectMany(p => p.Lines).OrderBy(p=> p.PageNumber).ThenBy(p=>p.LineNumber));
        var articleLines = new List<TaxLawDocumentLine>();
        var articles = new List<SilverDocument>();

        while (lines.Any())
        {
            var line = lines.Dequeue();
            if (line.LineNumber == 1) 
            {
                while (true)
                {
                    if (string.Equals(line.Content, "Secretaría de Servicios Parlamentarios"))
                    {
                        line = lines.Dequeue();
                        break;
                    }
                    line = lines.Dequeue();
                }                
            }

            string paginationTextPattern = @"\b\d+\sde\s\d+\b";
            bool isPaginationInfo = Regex.IsMatch(line.Content, paginationTextPattern);
            if(isPaginationInfo)
            {
                continue;
            }
            
            if (line.Content.StartsWith("Artículo") && !articleLines.Any())
            {
                articleLines.Add(line);
            }
            else if (line.Content.StartsWith("Artículo"))
            {
                var article = await ExtractArticleAsync(articleLines, cancellationToken);
                articles.Add(article);

                articleLines.Clear();
                articleLines.Add(line);                
            }
            else if (articleLines.Any())
            {
                articleLines.Add(line);
            }
        }

        return articles;
    }

    private async Task<SilverDocument> ExtractArticleAsync(IEnumerable<TaxLawDocumentLine> articleLines, CancellationToken cancellationToken)
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
            ArticleLawId = completionResult!.Article,
            ArticleContent = completionResult.Content            
        };

        (documentArticle.ArticleLines as List<TaxLawDocumentLine>)!.AddRange(articleLines);

        return documentArticle;
    }
}
