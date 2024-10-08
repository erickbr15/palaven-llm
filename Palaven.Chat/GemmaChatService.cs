using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Embeddings;
using Liara.Clients.Pinecone;
using Liara.Clients.Pinecone.Model;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Chat.Contracts;
using Palaven.Model.Chat;
using Palaven.Model.Data.Documents;

namespace Palaven.Chat;

public class GemmaChatService : IGemmaChatService
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IOpenAiServiceClient _openAiServiceClient;
    private readonly IDocumentRepository<GoldenDocument> _articleRepository;

    public GemmaChatService(
        IPineconeServiceClient pineconeServiceClient, 
        IOpenAiServiceClient openAiServiceClient,
        IDocumentRepository<GoldenDocument> documentRepository)
    {
        _pineconeServiceClient = pineconeServiceClient ?? throw new ArgumentNullException(nameof(pineconeServiceClient));
        _openAiServiceClient = openAiServiceClient ?? throw new ArgumentNullException(nameof(openAiServiceClient));
        _articleRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<ChatMessage> CreateAugmentedQueryPromptAsync(CreateAugmentedQueryPromptCommand command, CancellationToken cancellationToken)
    {
        var relatedArticles = await TryGetRelevantArticlesAsync(command, cancellationToken);
        
        if (!relatedArticles.Any())
        {
            return new ChatMessage
            {
                UserId = command.UserId.ToString(),
                Prompt = Resources.GemmaPromptTemplates.SimpleQuery.Replace("{instruction}", command.Query)
            };
        }

        var augmentedQuery = Resources.GemmaPromptTemplates.AugmentedQuery
            .Replace("{articles}", string.Join("", relatedArticles.Select(a => $"<article>{a.ArticleContent}</article>")))
            .Replace("{instruction}", command.Query);

        return new ChatMessage
        {
            UserId = command.UserId.ToString(),
            Prompt = augmentedQuery
        };
    }

    public ChatMessage CreateSimpleQueryPrompt(ChatMessage message)
    {
        return new ChatMessage
        {
            UserId = message.UserId,
            Prompt = Resources.GemmaPromptTemplates.SimpleQuery.Replace("{instruction}", message.Prompt)
        };
    }

    private async Task<IEnumerable<GoldenDocument>> TryGetRelevantArticlesAsync(CreateAugmentedQueryPromptCommand command, CancellationToken cancellationToken)
    {
        var createQueryEmbeddingsRequest = new CreateEmbeddingsModel
        {
            User = command.UserId.ToString(),
            Input = new List<string> { command.Query }
        };

        var queryEmbeddings = await _openAiServiceClient.CreateEmbeddingsAsync(createQueryEmbeddingsRequest, cancellationToken);
        if (queryEmbeddings == null || queryEmbeddings.Data == null || !queryEmbeddings.Data.Any())
        {
            return new List<GoldenDocument>();
        }

        var queryVectorsModel = new QueryVectorsModel
        {
            IncludeMetadata = true,
            TopK = command.TopK,
            Namespace = "palaven-sat",
            Vector = queryEmbeddings.Data[0].EmbeddingVector.Select(v => (double)v).ToList()
        };

        var queryVectorResult = await _pineconeServiceClient.QueryVectorsAsync(queryVectorsModel, cancellationToken);
        if (queryVectorResult == null || queryVectorResult.Matches == null || !queryVectorResult.Matches.Any(match => match.Score >= command.MinMatchScore))
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
