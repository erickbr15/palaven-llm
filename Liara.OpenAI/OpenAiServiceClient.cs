using Liara.Common.Http;
using Liara.OpenAI.Model.Chat;
using Liara.OpenAI.Model.Embeddings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Liara.OpenAI;

public class OpenAiServiceClient : IOpenAiServiceClient
{
    private readonly IHttpProxy _httpProxy;
    private readonly OpenAiOptions _openAiOptions;

    public OpenAiServiceClient(IOptions<OpenAiOptions> options, IHttpProxy httpProxy)
    {
        _openAiOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _httpProxy = httpProxy ?? throw new ArgumentNullException(nameof(httpProxy));
    }

    public async Task<ChatCompletionResponse?> CreateChatCompletionAsync(IEnumerable<Message> messages, ChatCompletionCreationModel inputModel, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_openAiOptions.ApiKey}" }
        };

        inputModel.Model = _openAiOptions.ChatGptModel;

        var chatCompletionBody = new ChatCompletionBodyBuilder().NewWith(messages, inputModel).Build();
        
        string content = JsonConvert.SerializeObject(chatCompletionBody);

        var bodyContent = new StringContent(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content)), Encoding.UTF8, "application/json");        

        var response = await _httpProxy.PostAsync(new Uri(_openAiOptions.ChatEndpointUrl),
            headers,
            bodyContent,
            cancellationToken);

        var completionResponse = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

        return completionResponse;
    }

    public async Task<CreateEmbeddingResponse?> CreateEmbeddingsAsync(CreateEmbeddingsModel inputModel, CancellationToken cancellationToken)
    {
        var headers = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {_openAiOptions.ApiKey}" }
        };

        var requestBody = new CreateEmbeddingsBodyBuilder().NewWithDefaults(_openAiOptions.EmbeddingsModel, inputModel.User, inputModel.Input).Build();
        
        string content = JsonConvert.SerializeObject(requestBody);

        var bodyContent = new StringContent(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(content)), Encoding.UTF8, "application/json");
        
        var response = await _httpProxy.PostAsync(new Uri(_openAiOptions.EmbeddingsEndpointUrl),
            headers,
            bodyContent,
            cancellationToken);

        var embeddings = await response.Content.ReadFromJsonAsync<dynamic>(cancellationToken: cancellationToken);

        var embeddingsArrayText = embeddings?.GetProperty("data").ToString();
        
        var embeddingResponse = new CreateEmbeddingResponse
        {
            Data = JsonConvert.DeserializeObject<List<Embedding>>(embeddingsArrayText)
        };

        return embeddingResponse;
    }
}
