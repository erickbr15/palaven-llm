using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Chat;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Palaven.Model.Documents;
using Palaven.Model.Documents.Golden;
using Palaven.Model.Ingest;
using System.Net;

namespace Palaven.Ingest.Commands;

public class CreateGoldenDocumentCommandHandler : ICommandHandler<CreateGoldenDocumentCommand, TaxLawDocumentIngestTask>
{
    private readonly IOpenAiServiceClient _openAiChatService;
    private readonly IDocumentRepository<TaxLawDocumentArticle> _articleDocumentRepository;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _goldenArticleDocumentRepository;

    public CreateGoldenDocumentCommandHandler(IOpenAiServiceClient openAiChatService,
        IDocumentRepository<TaxLawDocumentArticle> articleDocumentRepository,
        IDocumentRepository<TaxLawDocumentGoldenArticle> goldenArticleDocumentRepository)
    {
        _openAiChatService = openAiChatService ?? throw new ArgumentNullException(nameof(openAiChatService));
        _articleDocumentRepository = articleDocumentRepository ?? throw new ArgumentNullException(nameof(articleDocumentRepository));
        _goldenArticleDocumentRepository = goldenArticleDocumentRepository ?? throw new ArgumentNullException(nameof(goldenArticleDocumentRepository));
    }

    public async Task<IResult<TaxLawDocumentIngestTask?>> ExecuteAsync(CreateGoldenDocumentCommand inputModel, CancellationToken cancellationToken)
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

            var goldenArticle = new TaxLawDocumentGoldenArticle
            {
                Id = Guid.NewGuid().ToString(),
                TenantId = tenantId.ToString(),
                TraceId = inputModel.TraceId,
                LawId = article.LawId,
                LawName = "Ley del Impuesto Sobre la Renta",
                LawAcronym = "LISR",
                LawYear = 2024,
                ArticleId = inputModel.ArticleId,
                LawDocumentVersion = article.LawDocumentVersion,
                Article = article.Article,
                Content = article.Content,
                DocumentType = nameof(TaxLawDocumentGoldenArticle)
            };

            await PopulateOpenEndedQuestionInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateCloseEndedQuestionInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateExtractInformationInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateExtractReferencesInstructionsAsync(goldenArticle, cancellationToken);
            await PopulateSummarizationInstructionAsync(goldenArticle, cancellationToken);
            PopulateRetrievalAugmentationData(goldenArticle);


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

    private async Task PopulateOpenEndedQuestionInstructionsAsync(TaxLawDocumentGoldenArticle goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptOpenEndQuestionTemplate;
        
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptOpenEndQuestionTemplate
            .Replace("{article}", goldenArticle.Content)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.6m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenArticleFineTuningInstructions(goldenArticle, instructions.Instructions, "open_end-question", new List<string> { "question-answering", "open_end-question" });        
    }

    private async Task PopulateCloseEndedQuestionInstructionsAsync(TaxLawDocumentGoldenArticle goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptClosedEndQuestionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptClosedEndQuestionTemplate
            .Replace("{article}", goldenArticle.Content)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.6m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenArticleFineTuningInstructions(goldenArticle, instructions.Instructions, "close_end-question", new List<string> { "question-answering", "close_end-question" });
    }

    private async Task PopulateExtractInformationInstructionsAsync(TaxLawDocumentGoldenArticle goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptInformationExtractionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptInformationExtractionTemplate
            .Replace("{article}", goldenArticle.Content)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenArticleFineTuningInstructions(goldenArticle, instructions.Instructions, "extract_information", new List<string> { "question-answering", "extract_information" });
    }

    private async Task PopulateExtractReferencesInstructionsAsync(TaxLawDocumentGoldenArticle goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptReferencesExtractionTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptReferencesExtractionTemplate
            .Replace("{article}", goldenArticle.Content)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenArticleFineTuningInstructions(goldenArticle, instructions.Instructions, "extract_references", new List<string> { "question-answering", "extract_references" });
    }

    private async Task PopulateSummarizationInstructionAsync(TaxLawDocumentGoldenArticle goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptSummarizationTemplate;
        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptSummarizationTemplate
            .Replace("{article}", goldenArticle.Content)
            .Replace("{law}", goldenArticle.LawName)
            .Replace("{year}", goldenArticle.LawYear.ToString());

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenArticleFineTuningInstructions(goldenArticle, instructions.Instructions, "summarization", new List<string> { "summarization" });
    }

    private void PopulateRetrievalAugmentationData(TaxLawDocumentGoldenArticle goldenArticle)
    {
        foreach (var instruction in goldenArticle.FineTuningData)
        {
            goldenArticle.RetrievalAugmentationData.Add(new RagInstruction
            {
                InstructionId = instruction.InstructionId,
                Instruction = instruction.Instruction,
                Metadata = instruction.Metadata
            });
        }

        var goldenArticleMetadata = goldenArticle.FineTuningData[0].Metadata;

        goldenArticle.RetrievalAugmentationData.Add(new RagInstruction
        {
            Instruction = $"Sumariza el {goldenArticleMetadata.Article} de la {goldenArticleMetadata.LawName} del año {goldenArticleMetadata.LawYear}",
            Metadata = goldenArticleMetadata
        });

        goldenArticle.RetrievalAugmentationData.Add(new RagInstruction
        {
            Instruction = $"Realiza un resumen del {goldenArticleMetadata.Article}  de la  {goldenArticleMetadata.LawName} del año {goldenArticleMetadata.LawYear}",
            Metadata = goldenArticleMetadata
        });

        goldenArticle.RetrievalAugmentationData.Add(new RagInstruction
        {
            Instruction = $"Resume el {goldenArticleMetadata.Article}  de la  {goldenArticleMetadata.LawName} del año {goldenArticleMetadata.LawYear}",
            Metadata = goldenArticleMetadata
        });
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

    private static void PopulateGoldenArticleFineTuningInstructions(TaxLawDocumentGoldenArticle goldenArticle, IList<ChatCompletionInstruction> instructions, string category, IList<string> llmFunctions)
    {
        foreach (var instruction in instructions)
        {
            goldenArticle.FineTuningData.Add(new FineTuningInstruction
            {
                InstructionId = Guid.NewGuid(),
                Instruction = instruction.Instruction,
                Category = category,
                Response = instruction.Response,
                Metadata = new InstructionMetadata
                {
                    LawId = goldenArticle.LawId,
                    LawName = goldenArticle.LawName,
                    LawAcronym = goldenArticle.LawAcronym,
                    LawYear = goldenArticle.LawYear,
                    ArticleId = goldenArticle.ArticleId,
                    Article = goldenArticle.Article,
                    LlmFunctions = llmFunctions
                }
            });
        }
    }
}
