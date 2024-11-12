using Liara.Integrations.OpenAI;
using Liara.Integrations.OpenAI.Chat;
using Microsoft.Extensions.Options;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Model.AI.Llm;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Infrastructure.Llm;

public class ChatGptChatService : IChatService
{
    private readonly OpenAIChatOptions _options;
    private readonly IPromptEngineeringService<IEnumerable<Message>> _promptEngineeringService;
    private readonly IRetrievalService _palavenRetrievalService;
    private readonly IOpenAIChatService _openAIChatService;

    public ChatGptChatService(IOptions<OpenAIChatOptions> optionsService, IPromptEngineeringService<IEnumerable<Message>> promptEngineeringService, 
        IRetrievalService retrievalService,
        IOpenAIChatService openAIChatService)
    {
        _options = optionsService?.Value ?? throw new ArgumentNullException(nameof(optionsService));
        _promptEngineeringService = promptEngineeringService ?? throw new ArgumentNullException(nameof(promptEngineeringService));
        _palavenRetrievalService = retrievalService ?? throw new ArgumentNullException(nameof(retrievalService));
        _openAIChatService = openAIChatService ?? throw new ArgumentNullException(nameof(openAIChatService));
    }

    public async Task<string> GetChatResponseAsync(string query, CancellationToken cancellationToken)
    {
        var relatedDocuments = await _palavenRetrievalService.RetrieveRelatedDocumentsAsync<GoldenDocument>(new List<string> { query }, new RetrievalOptions(), cancellationToken);
        
        var promptMessages = _promptEngineeringService.CreateAugmentedQueryPrompt(query, relatedDocuments);

        var completionModel = new ChatCompletionCreationModel
        {
            Model = _options.Model,
            Temperature = _options.Temperature
        };

        var chatCompletion = await _openAIChatService.CreateChatCompletionAsync(promptMessages, completionModel, cancellationToken);

        var chatResponse = chatCompletion!.Choices.Any() ? chatCompletion.Choices[0].Message.Content : string.Empty;

        return chatResponse;
    }
}
