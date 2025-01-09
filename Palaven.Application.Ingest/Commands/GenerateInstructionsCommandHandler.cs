using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Integrations.OpenAI.Chat;
using Newtonsoft.Json;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Liara.Common;
using Liara.Common.Abstractions.Persistence;
using Liara.Integrations.OpenAI;
using Palaven.Infrastructure.Model.AI;

namespace Palaven.Application.Ingest.Commands;

public class GenerateInstructionsCommandHandler : ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult>
{
    private readonly IDocumentRepository<SilverDocument> _silverDocumentRepository;
    private readonly IDocumentRepository<GoldenDocument> _goldenDocumentRepository;
    private readonly IOpenAIChatService _openAIChatService;

    public GenerateInstructionsCommandHandler(IDocumentRepository<SilverDocument> silverDocumentRepository, IDocumentRepository<GoldenDocument> goldenDocumentRepository,
        IOpenAIChatService openAIChatService)
    {
        _silverDocumentRepository = silverDocumentRepository ?? throw new ArgumentNullException(nameof(silverDocumentRepository));
        _goldenDocumentRepository = goldenDocumentRepository ?? throw new ArgumentNullException(nameof(goldenDocumentRepository));
        _openAIChatService = openAIChatService ?? throw new ArgumentNullException(nameof(openAIChatService));
    }

    public async Task<IResult<InstructionGenerationResult>> ExecuteAsync(GenerateInstructionsCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return Result<InstructionGenerationResult>.Fail(new ArgumentNullException(nameof(command)));
        }

        var silverDocumens = await FetchSilverDocumentsAsync(command.OperationId, command.DocumentIds, cancellationToken);
        if (!silverDocumens.Any())
        {
            return Result<InstructionGenerationResult>.Fail(new InvalidOperationException("Unable to find the silver documents"));
        }

        var result = new InstructionGenerationResult { OperationId = command.OperationId };
        foreach (var silverDocument in silverDocumens)
        {
            GoldenDocument? goldenDocument = null;
            try
            {
                goldenDocument = await CreateGoldenDocumentAsync(silverDocument, cancellationToken);
            }
            catch
            {
                result.FailedSilverDocumentIds.Add(silverDocument.Id);
            }

            if(goldenDocument != null)
            {
                try
                {
                    await _goldenDocumentRepository.CreateAsync(goldenDocument, goldenDocument.TenantId.ToString(), cancellationToken);
                    result.SuccessfulDocumentIds.Add(goldenDocument.Id);
                }
                catch
                {
                    result.FailedSilverDocumentIds.Add(silverDocument.Id);
                }
            }
        }

        return Result<InstructionGenerationResult>.Success(result);
    }

    private async Task<GoldenDocument> CreateGoldenDocumentAsync(SilverDocument silverDocument, CancellationToken cancellationToken)
    {
        var goldenDocument = new GoldenDocument
        {
            Id = Guid.NewGuid(),
            TenantId = silverDocument.TenantId,
            TraceId = silverDocument.TraceId,
            ArticleId = silverDocument.ArticleId,
            ArticleContent = silverDocument.ArticleContent,
            DocumentSchema = nameof(GoldenDocument)
        };        

        silverDocument.Metadata.ToList().ForEach(m =>
        {
            goldenDocument.Metadata.Add(m.Key, m.Value);
        });

        silverDocument.Paragraphs.ToList().ForEach(goldenDocument.Paragraphs.Add);

        await PopulateOpenEndedQuestionInstructionsAsync(goldenDocument, cancellationToken);
        await PopulateCloseEndedQuestionInstructionsAsync(goldenDocument, cancellationToken);
        await PopulateExtractInformationInstructionsAsync(goldenDocument, cancellationToken);
        await PopulateExtractReferencesInstructionsAsync(goldenDocument, cancellationToken);
        await PopulateSummarizationInstructionAsync(goldenDocument, cancellationToken);

        return goldenDocument;
    }

    private async Task<IList<SilverDocument>> FetchSilverDocumentsAsync(Guid operationId, Guid[] documentIds, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM c WHERE c.trace_id = '{operationId}' AND c.id IN ({string.Join(",", documentIds.Select(d => $"'{d}'"))})";
        var silverDocuments = await _silverDocumentRepository.GetAsync(query, continuationToken: null, cancellationToken);

        return silverDocuments.ToList();
    }

    private async Task PopulateOpenEndedQuestionInstructionsAsync(GoldenDocument goldenArticle, CancellationToken cancellationToken)
    {
        var systemPrompt = Resources.ChatGptPromptTemplates.SystemPromptOpenEndQuestionTemplate;

        var userPrompt = Resources.ChatGptPromptTemplates.UserPromptOpenEndQuestionTemplate
            .Replace("{article}", goldenArticle.ArticleContent)
            .Replace("{law}", goldenArticle.Metadata[EtlMetadataKeys.LawName])
            .Replace("{year}", goldenArticle.Metadata[EtlMetadataKeys.LawYear]);

        var temperature = 0.7m;
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
            .Replace("{law}", goldenArticle.Metadata[EtlMetadataKeys.LawName])
            .Replace("{year}", goldenArticle.Metadata[EtlMetadataKeys.LawYear]);

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
            .Replace("{law}", goldenArticle.Metadata[EtlMetadataKeys.LawName])
            .Replace("{year}", goldenArticle.Metadata[EtlMetadataKeys.LawYear]);

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
            .Replace("{law}", goldenArticle.Metadata[EtlMetadataKeys.LawName])
            .Replace("{year}", goldenArticle.Metadata[EtlMetadataKeys.LawYear]);

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
            .Replace("{law}", goldenArticle.Metadata[EtlMetadataKeys.LawName])
            .Replace("{year}", goldenArticle.Metadata[EtlMetadataKeys.LawYear]);

        var temperature = 0.0m;

        var instructions = await InvokeChatGptAsync(systemPrompt, userPrompt, temperature, cancellationToken);

        if (!(instructions?.Success ?? false))
        {
            return;
        }

        PopulateGoldenDocumentInstructions(goldenArticle, instructions.Instructions, "summarization");
    }

    private async Task<OpenAIInstructionsGenerationResult?> InvokeChatGptAsync(string systemPrompt, string userPrompt, decimal? temperature, CancellationToken cancellationToken)
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

        var chatGptResponse = await _openAIChatService.CreateChatCompletionAsync(messages, chatGptCallOptions, cancellationToken);

        var completionResult = JsonConvert.DeserializeObject<OpenAIInstructionsGenerationResult>(chatGptResponse!
            .Choices
            .FirstOrDefault()?
            .Message.Content ?? string.Empty);

        return completionResult;
    }

    private static void PopulateGoldenDocumentInstructions(GoldenDocument goldenArticle, IList<OpenAIGeneratedInstruction> instructions, string instructionType)
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
