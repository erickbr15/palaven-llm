using Liara.CosmosDb;
using Liara.OpenAI;
using Liara.OpenAI.Model.Embeddings;
using Liara.Pinecone;
using Liara.Pinecone.Model;
using Microsoft.Azure.Cosmos;
using Palaven.Chat.Contracts;
using Palaven.Model.Chat;
using Palaven.Model.Ingest.Documents.Golden;

namespace Palaven.Chat;

public class GemmaChatService : IGemmaChatService
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IOpenAiServiceClient _openAiServiceClient;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _articleRepository;

    public GemmaChatService(
        IPineconeServiceClient pineconeServiceClient, 
        IOpenAiServiceClient openAiServiceClient,
        IDocumentRepository<TaxLawDocumentGoldenArticle> documentRepository)
    {
        _pineconeServiceClient = pineconeServiceClient ?? throw new ArgumentNullException(nameof(pineconeServiceClient));
        _openAiServiceClient = openAiServiceClient ?? throw new ArgumentNullException(nameof(openAiServiceClient));
        _articleRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<ChatMessage> AugmentQueryAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var relatedArticles = await TryGetRelevantArticlesAsync(message, cancellationToken);
        
        if (!relatedArticles.Any())
        {
            return new ChatMessage
            {
                UserId = message.UserId,
                Query = Resources.GemmaPromptTemplates.SimpleQuery.Replace("{instruction}", message.Query)
            };
        }

        var augmentedQuery = Resources.GemmaPromptTemplates.AugmentedQuery
            .Replace("{articles}", string.Join("", relatedArticles.Select(a => $"<article>{a.Content}</article>")))
            .Replace("{instruction}", message.Query);

        return new ChatMessage
        {
            UserId = message.UserId,
            Query = augmentedQuery
        };
    }

    private async Task<IEnumerable<TaxLawDocumentGoldenArticle>> TryGetRelevantArticlesAsync(ChatMessage message, CancellationToken cancellationToken)
    {
        var createQueryEmbeddingsRequest = new CreateEmbeddingsModel
        {
            User = message.UserId,
            Input = new List<string> { message.Query }
        };

        var queryEmbeddings = await _openAiServiceClient.CreateEmbeddingsAsync(createQueryEmbeddingsRequest, cancellationToken);
        if (queryEmbeddings == null || queryEmbeddings.Data == null || !queryEmbeddings.Data.Any())
        {
            return new List<TaxLawDocumentGoldenArticle>();
        }

        var queryVectorsModel = new QueryVectorsModel
        {
            IncludeMetadata = true,
            TopK = 3,
            Namespace = "palaven-sat",
            Vector = queryEmbeddings.Data[0].EmbeddingVector.Select(v => (double)v).ToList()
        };

        var queryVectorResult = await _pineconeServiceClient.QueryVectorsAsync(queryVectorsModel, cancellationToken);
        if (queryVectorResult == null || queryVectorResult.Matches == null || !queryVectorResult.Matches.Any(match => match.Score >= 0.8))
        {
            return new List<TaxLawDocumentGoldenArticle>();
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
