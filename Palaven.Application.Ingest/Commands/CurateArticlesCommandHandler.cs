using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Integrations.OpenAI;
using Liara.Integrations.OpenAI.Chat;
using Liara.Persistence.Abstractions;
using Newtonsoft.Json;
using Palaven.Application.Ingest.Resources;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Commands;

public class CurateArticlesCommandHandler : ICommandHandler<CurateArticlesCommand, ArticlesCurationResult>
{
    private readonly IOpenAIChatService _openAiChatService;
    private readonly IDocumentRepository<SilverDocument> _silverDocumentRepository;
    
    public CurateArticlesCommandHandler(IOpenAIChatService openAIChatService, IDocumentRepository<SilverDocument> silverDocumentRepository)
    {
        _openAiChatService = openAIChatService ?? throw new ArgumentNullException(nameof(openAIChatService));
        _silverDocumentRepository = silverDocumentRepository ?? throw new ArgumentNullException(nameof(silverDocumentRepository));
    }

    public async Task<IResult<ArticlesCurationResult>> ExecuteAsync(CurateArticlesCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return Result<ArticlesCurationResult>.Fail(new ArgumentNullException(nameof(command)));
        }

        var query = $"SELECT * FROM c WHERE c.trace_id = '{command.OperationId}' AND c.id IN ({string.Join(",", command.DocumentIds.Select(d => $"'{d}'"))})";
        var documents = await _silverDocumentRepository.GetAsync(query, continuationToken: null, cancellationToken);

        var silverDocuments = new List<SilverDocument>(documents);
        
        var result = new ArticlesCurationResult { OperationId = command.OperationId };

        foreach (var silverDocument in silverDocuments)
        {
            try
            {
                var (articleLawId, articleContent) = await CurateSilverDocumentAsync(silverDocument, cancellationToken);

                if(IsArticleLawIdValid(articleLawId))
                {
                    if (string.IsNullOrEmpty(articleLawId) || string.IsNullOrEmpty(articleContent))
                    {
                        silverDocument.CuratedByAIModel = false;
                    }
                    else
                    {
                        silverDocument.CuratedByAIModel = true;
                        silverDocument.ArticleId = articleLawId;
                        silverDocument.ArticleContent = articleContent;
                    }

                    await _silverDocumentRepository.UpsertAsync(silverDocument, silverDocument.TenantId.ToString(), cancellationToken);

                    result.CuratedDocumentIds.Add(silverDocument.Id);
                }
                else
                {
                    result.InvalidDocumentsCount += 1;
                }
            }
            catch
            {
                //Ignored for now. Next version needs to log this error
                result.FailedDocumentIds.Add(silverDocument.Id);
            }
        }

        return Result<ArticlesCurationResult>.Success(result);
    }    

    private async Task<(string ArticleLawId, string ArticleContent)> CurateSilverDocumentAsync(SilverDocument document, CancellationToken cancellationToken)
    {
        var articleContent = string.Join(Environment.NewLine, document.Paragraphs.Select(l => l.Content.Trim()));

        var chatGptCallOptions = new ChatCompletionCreationModel
        {
            Model = "gpt-3.5-turbo",
            Temperature = 0.0m,
            User = document.TenantId.ToString(),
            ResponseFormat = new ResponseFormat { Type = "json_object" }
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

        var completionResult = JsonConvert.DeserializeObject<OpenAIArticleExtractionResult>(chatGptResponse!
            .Choices
            .FirstOrDefault()?
            .Message.Content ?? string.Empty);
        

        return (completionResult?.Article ?? string.Empty, completionResult?.Content ?? string.Empty);
    }

    private bool IsArticleLawIdValid(string articleLawId)
    {
        var invalidArticleLawIds = Etl.TaxLawInvalidArticleIds.Split("|", StringSplitOptions.RemoveEmptyEntries);
        return !string.IsNullOrEmpty(articleLawId) && !invalidArticleLawIds.Contains(articleLawId);        
    }
}
