using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Chat;
using Liara.Clients.OpenAI.Model.Embeddings;
using Liara.Clients.Pinecone;
using Liara.Clients.Pinecone.Model;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Chat.Contracts;
using Palaven.Model.Chat;
using Palaven.Model.Documents;

namespace Palaven.Chat;

public class OpenAIChatService : IOpenAIChatService
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IOpenAiServiceClient _openAiServiceClient;
    private readonly IDocumentRepository<GoldenDocument> _articleRepository;

    public OpenAIChatService(IPineconeServiceClient pineconeServiceClient, IOpenAiServiceClient openAiServiceClient, IDocumentRepository<GoldenDocument> articleRepository)
    {
        _pineconeServiceClient = pineconeServiceClient ?? throw new ArgumentNullException(nameof(pineconeServiceClient));
        _openAiServiceClient = openAiServiceClient ?? throw new ArgumentNullException(nameof(openAiServiceClient));
        _articleRepository = articleRepository ?? throw new ArgumentNullException(nameof(articleRepository));
    }

    public async Task<string> GetChatResponseAsync(ChatMessage chatMessage, CancellationToken cancellationToken)
    {            
        var relatedArticles = await TryGetRelevantArticlesAsync(chatMessage, cancellationToken);

        var messages = BuildChatMessages(chatMessage.Prompt, relatedArticles);

        var completionModel = new ChatCompletionCreationModel
        {
            Model = "gpt-4",
            User = chatMessage.UserId,
            Temperature = 0.65m
        };

        var chatCompletion = await _openAiServiceClient.CreateChatCompletionAsync(messages, completionModel, cancellationToken);

        var chatResponse = chatCompletion!.Choices.Any() ? chatCompletion.Choices[0].Message.Content : string.Empty;

        return chatResponse;
    }

    private IEnumerable<Message> BuildChatMessages(string query, IEnumerable<GoldenDocument> relatedArticles)
    {
        const string userQueryMark = "{user_query}";
        const string userAdditionalInfoMark = "{additional_info}";

        var additionalInformation = relatedArticles.Any() ? $"<additional_info>{string.Join("\n\n\n", relatedArticles.Select(r=>r.ArticleContent))}</additional_info>" : string.Empty;

        var userMessageContent = Resources.ChatGptPromptTemplates.AnswerMexicanTaxLawQuestionUserRole
            .Replace(userQueryMark, query)
            .Replace(userAdditionalInfoMark, additionalInformation);

        var messages = new List<Message>
        {
            new()
            {
                Role = "system",
                Content = Resources.ChatGptPromptTemplates.AnswerMexicanTaxLawQuestionSystemRole
            },
            new()
            {
                Role = "user",
                Content = userMessageContent
            },
        };

        return messages;
    }

    private async Task<IEnumerable<GoldenDocument>> TryGetRelevantArticlesAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var createQueryEmbeddingsRequest = new CreateEmbeddingsModel
        {
            User = message.UserId,
            Input = new List<string> { message.Prompt }
        };

        var queryEmbeddings = await _openAiServiceClient.CreateEmbeddingsAsync(createQueryEmbeddingsRequest, cancellationToken);
        if(queryEmbeddings == null || queryEmbeddings.Data == null || !queryEmbeddings.Data.Any())
        {
            return new List<GoldenDocument>();
        }

        var queryVectorsModel = new QueryVectorsModel
        {
            IncludeMetadata = true,
            TopK = 3,
            Namespace = "palaven-sat",
            Vector = queryEmbeddings.Data[0].EmbeddingVector.Select(v => (double)v).ToList()
        };

        var queryVectorResult = await _pineconeServiceClient.QueryVectorsAsync(queryVectorsModel, cancellationToken);
        if(queryVectorResult == null || queryVectorResult.Matches == null || !queryVectorResult.Matches.Any(match=>match.Score >= 0.8))
        {
            return new List<GoldenDocument>();
        }

        var articleIds = queryVectorResult.Matches
            .SelectMany(m => m.Metadata)
            .Where(m => m.Key == "golden_article_id")
            .Select(m => m.Value.ToString())
            .Distinct()
            .ToList();

        var goldenArticlesQuery = $"SELECT * FROM c WHERE c.id IN ({string.Join(",", articleIds.Select(a => $"'{a}'"))})";

        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var goldenArticles = await _articleRepository.GetAsync(new QueryDefinition(goldenArticlesQuery),
            continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken: cancellationToken);

        return goldenArticles.ToList();
    }
}
