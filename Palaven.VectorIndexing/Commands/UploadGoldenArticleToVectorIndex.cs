using Liara.Common;
using Liara.CosmosDb;
using Liara.Pinecone;
using Liara.Pinecone.Model;
using Microsoft.Azure.Cosmos;
using Palaven.Model.Ingest.Documents.Golden;
using Palaven.Model.VectorIndexing.Commands;

namespace Palaven.VectorIndexing.Commands;

public class UploadGoldenArticleToVectorIndex : ITraceableCommand<UploadGoldenArticleToVectorIndexModel, Guid>
{
    private readonly IPineconeServiceClient _pineconeServiceClient;
    private readonly IDocumentRepository<TaxLawDocumentGoldenArticle> _goldenArticleRepository;

    public UploadGoldenArticleToVectorIndex(IPineconeServiceClient pineconeServiceClient, IDocumentRepository<TaxLawDocumentGoldenArticle> goldenArticleRepository)
    {
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

        var upsertDataModel = BuildUpsertDataModel(goldenArticle);

        await _pineconeServiceClient.UpsertAsync(upsertDataModel, cancellationToken);

        return Result<Guid>.Success(traceId);
    }

    private static UpsertDataModel BuildUpsertDataModel(TaxLawDocumentGoldenArticle goldenArticle)
    {
        var upsertDataBuilder = new GoldenArticleUpsertDataBuilder().NewWithNamespace("palaven-sat");

        upsertDataBuilder.AddVector(
            goldenArticle.AugmentationData.Summary.Embeddings,
            new Guid(goldenArticle.Id),
            goldenArticle.AugmentationData.Summary.Metadata);

        foreach (var question in goldenArticle.AugmentationData.Questions)
        {
            upsertDataBuilder.AddVector(question.Embeddings, new Guid(goldenArticle.Id), question.Metadata);
        }

        return upsertDataBuilder.Build()!;
    }
}
