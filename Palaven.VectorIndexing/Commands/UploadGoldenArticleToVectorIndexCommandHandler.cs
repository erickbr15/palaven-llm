using Liara.Clients.OpenAI;
using Liara.Clients.OpenAI.Model.Embeddings;
using Liara.Clients.Pinecone;
using Liara.Clients.Pinecone.Model;
using Liara.Common;
using Liara.CosmosDb;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Documents;
using Palaven.Model.Documents.Metadata;
using Palaven.Model.VectorIndexing;

namespace Palaven.VectorIndexing.Commands;

public class UploadGoldenArticleToVectorIndexCommandHandler : ICommandHandler<UploadGoldenArticleToVectorIndexCommand, Guid>
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IOpenAiServiceClient _openAiServiceClient;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleRepository;

    public UploadGoldenArticleToVectorIndexCommandHandler(IOpenAiServiceClient openAiServiceClient, 
        IPineconeServiceClient pineconeServiceClient, 
        IDocumentRepository<GoldenDocument> goldenArticleRepository)
    {
        _openAiServiceClient = openAiServiceClient ?? throw new ArgumentNullException(nameof(openAiServiceClient));
        _pineconeServiceClient = pineconeServiceClient ?? throw new ArgumentNullException(nameof(pineconeServiceClient));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
    }
    
    public async Task<IResult<Guid>> ExecuteAsync(UploadGoldenArticleToVectorIndexCommand command, CancellationToken cancellationToken)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = \"{command.GoldenArticleId}\"");

        var queryResults = await _goldenArticleRepository.GetAsync(query, continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var goldenArticle = queryResults.SingleOrDefault() ?? throw new InvalidOperationException($"Unable to find the tax law golden article with id {command.GoldenArticleId}");

        var upsertDataModel = await BuildUpsertDataModel(goldenArticle);

        await _pineconeServiceClient.UpsertAsync(upsertDataModel, cancellationToken);

        return Result<Guid>.Success(command.TraceId);
    }

    private async Task<UpsertDataModel> BuildUpsertDataModel(GoldenDocument goldenArticle)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");
        var vectors = new List<Vector>();

        foreach (var item in goldenArticle.Instructions)
        {
            var createEmbeddingsModel = new CreateEmbeddingsModel
            {
                User = tenantId.ToString(),
                Input = new List<string> { item.InstructionText }
            };

            var embeddings = await _openAiServiceClient.CreateEmbeddingsAsync(createEmbeddingsModel, CancellationToken.None);

            //TODO: Implement InstructionMetadata factory
            var vector = (new PineconeVectorBuilder().NewWith(embeddings!.Data[0].EmbeddingVector, goldenArticle.Id, new InstructionMetadata())).Build();

            vectors.Add(vector);
        }
        
        var upsertDataModel = new UpsertDataModel
        {
            Namespace = "palaven-sat",
            Vectors = vectors.ToList()
        };

        return upsertDataModel;
    }
}
