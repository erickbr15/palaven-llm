using Liara.Common;
using Liara.CosmosDb;
using Liara.OpenAI;
using Liara.OpenAI.Model.Embeddings;
using Liara.Pinecone;
using Liara.Pinecone.Model;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Ingest.Documents.Golden;
using Palaven.Model.VectorIndexing.Commands;
using System.Collections.Concurrent;

namespace Palaven.VectorIndexing.Commands;

public class UploadGoldenArticleToVectorIndex : ITraceableCommand<UploadGoldenArticleToVectorIndexModel, Guid>
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IOpenAiServiceClient _openAiServiceClient;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _goldenArticleRepository;

    public UploadGoldenArticleToVectorIndex(
        IOpenAiServiceClient openAiServiceClient, 
        IPineconeServiceClient pineconeServiceClient, IDocumentRepository<TaxLawDocumentGoldenArticle> goldenArticleRepository)
    {
        _openAiServiceClient = openAiServiceClient ?? throw new ArgumentNullException(nameof(openAiServiceClient));
        _pineconeServiceClient = pineconeServiceClient ?? throw new ArgumentNullException(nameof(pineconeServiceClient));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
    }

    public async Task<IResult<Guid>> ExecuteAsync(Guid traceId, UploadGoldenArticleToVectorIndexModel inputModel, CancellationToken cancellationToken)
    {        
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = \"{inputModel.GoldenArticleId}\"");

        var queryResults = await _goldenArticleRepository.GetAsync(query, continuationToken: null,
            new QueryRequestOptions { PartitionKey = new PartitionKey(tenantId.ToString()) },
            cancellationToken);

        var goldenArticle = queryResults.SingleOrDefault() ?? throw new InvalidOperationException($"Unable to find the tax law golden article with id {inputModel.GoldenArticleId}");

        var upsertDataModel = await BuildUpsertDataModel(goldenArticle);
        
        await _pineconeServiceClient.UpsertAsync(upsertDataModel, cancellationToken);

        return Result<Guid>.Success(traceId);
    }    
    
    private async Task<UpsertDataModel> BuildUpsertDataModel(TaxLawDocumentGoldenArticle goldenArticle)
    {
        var tenantId = new Guid("69A03A54-4181-4D50-8274-D2D88EA911E4");
        var vectors = new List<Vector>();

        foreach (var item in goldenArticle.RetrievalAugmentationData)
        {
            var createEmbeddingsModel = new CreateEmbeddingsModel
            {
                User = tenantId.ToString(),
                Input = new List<string> { item.Instruction }
            };

            var embeddings = await _openAiServiceClient.CreateEmbeddingsAsync(createEmbeddingsModel, CancellationToken.None);
            var vector = (new PineconeVectorBuilder().NewWith(embeddings!.Data[0].EmbeddingVector, goldenArticle.Id, item.Metadata)).Build();

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
