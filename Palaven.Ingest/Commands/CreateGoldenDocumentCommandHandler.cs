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

public class CreateGoldenDocumentCommandHandler : ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask>
{
    private readonly IOpenAiServiceClient _openAiChatService;
    private readonly IDocumentRepository<SilverDocument> _articleDocumentRepository;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleDocumentRepository;

    public CreateGoldenDocumentCommandHandler(IOpenAiServiceClient openAiChatService,
        IDocumentRepository<SilverDocument> articleDocumentRepository,
        IDocumentRepository<GoldenDocument> goldenArticleDocumentRepository)
    {
        _openAiChatService = openAiChatService ?? throw new ArgumentNullException(nameof(openAiChatService));
        _articleDocumentRepository = articleDocumentRepository ?? throw new ArgumentNullException(nameof(articleDocumentRepository));
        _goldenArticleDocumentRepository = goldenArticleDocumentRepository ?? throw new ArgumentNullException(nameof(goldenArticleDocumentRepository));
    }

    public async Task<IResult<TaxLawDocumentIngestTask>> ExecuteAsync(CreateGoldenDocumentCommand inputModel, CancellationToken cancellationToken)
    {
        try
        {
            var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = \"{inputModel.ArticleId}\"");

            var queryResults = await _articleDocumentRepository.GetAsync(query, continuationToken: null,
                new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
                cancellationToken);

            var article = queryResults.SingleOrDefault() ?? throw new InvalidOperationException($"Unable to find the tax law document article with id {inputModel.ArticleId}");

            var queryGoldenArticle = new QueryDefinition($"SELECT * FROM c WHERE c.ArticleId = \"{inputModel.ArticleId}\"");

            var queryGoldenArticleResults = await _goldenArticleDocumentRepository.GetAsync(queryGoldenArticle, continuationToken: null,
                new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
                cancellationToken);

            if (queryGoldenArticleResults.Any())
            {
                return Result<TaxLawDocumentIngestTask>.Success(new TaxLawDocumentIngestTask());
            }            

            var goldenArticle = new GoldenDocument
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId.ToString(),
                TraceId = inputModel.TraceId,
                LawId = article.LawId,
                LawName = "Ley del Impuesto Sobre la Renta",
                LawAcronym = "LISR",
                LawYear = 2024,                
                LawDocumentVersion = article.LawDocumentVersion,
                ArticleLawId = article.ArticleLawId,
                ArticleContent = article.ArticleContent,
                DocumentType = nameof(GoldenDocument)
            };

            await PopulateOpenEndedQuestionInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateCloseEndedQuestionInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateExtractInformationInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateExtractReferencesInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateSummarizationInstructionAsync(goldenArticle, cancellationToken);

            var result = await _goldenArticleDocumentRepository.CreateAsync(goldenArticle, new PartitionKey(tenantId.ToString()), itemRequestOptions: null, cancellationToken);

            if (result.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException($"Unable to create the account file document. Status code: {result.StatusCode}");
            }

            return Result<TaxLawDocumentIngestTask>.Success(new TaxLawDocumentIngestTask());
        }
        catch (Exception ex)
        {
            return Result<TaxLawDocumentIngestTask>.Fail(new List<ValidationError>(), new List<Exception> { ex });
        }        
    }

    private async Task PopulateOpenEndedQuestionInstructionsAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptOpenEndQuestionTemplate;
        
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptOpenEndQuestionTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.6m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "open_end-question");
    }

    private async Task PopulateCloseEndedQuestionInstructionsAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptClosedEndQuestionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptClosedEndQuestionTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.6m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "close_end-question");
    }

    private async Task PopulateExtractInformationInstructionsAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptInformationExtractionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptInformationExtractionTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "extract_information");
    }

    private async Task PopulateExtractReferencesInstructionsAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptReferencesExtractionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptReferencesExtractionTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "extract_references");
    }

    private async Task PopulateSummarizationInstructionAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptSummarizationTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptSummarizationTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "summarization");
    }    

    private async Task<ChatCompletionInstructionsResponse?> InvokeChatGptAsync(string systemPrompt, string userPrompt, decimal? temperature, CancellationToken cancellationToken)
    {
        var tenanId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");
        
        var chatGptCallOptions = new ChatCompletionCreationModel
        {
            Temperature = temperature,
            User = tenanId.ToString(),
            ResponseFormat = new ResponseFormat { Type = "json_object" }
        };

        var messages = new List<Message>
        {
            new()
            {
                Role = "system",
                Content = systemPrompt
            },
            new()
            {
                Role = "user",
                Content = userPrompt
            }
        };

        var chatGptResponse = await _openAiChatService.CreateChatCompletionAsync(messages, chatGptCallOptions, cancellationToken);

        var completionResult = JsonConvert.DeserializeObject<ChatCompletionInstructionsResponse>(
                chatGptResponse!.Choices.FirstOrDefault()?.Message.Content ?? string.Empty);

        return completionResult;
    }

    private static void PopulateGoldenDocumentInstructions(GoldenDocument goldenArticle, IList<ChatCompletionInstruction> instructions, string instructionType)
    {
        foreach (var instruction in instructions)
        {
            goldenArticle.Instructions.Add(new Instruction
            {
                InstructionId = Guid.NewGuid(),
                InstructionText = instruction.Instruction,
                Type = instructionType,
                Response = instruction.Response
            });
        }
    }
}
