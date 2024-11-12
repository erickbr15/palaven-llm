using Liara.Integrations.OpenAI;
using Liara.Integrations.OpenAI.Embeddings;
using Liara.Integrations.Pinecone;
using Newtonsoft.Json.Linq;
using Palaven.Infrastructure.Abstractions.VectorIndexing;
using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.VectorIndexing;

namespace Palaven.Infrastructure.VectorIndexing;

public class PineconeVectorIndexService : IVectorIndexService
{
    private readonly IPineconeService _pineconeService;
    private readonly IOpenAIEmbeddingsService _openAIEmbeddingsService;

    public PineconeVectorIndexService(IPineconeService pineconeService, IOpenAIEmbeddingsService openAIEmbeddingsService)
    {
        _pineconeService = pineconeService ?? throw new ArgumentNullException(nameof(pineconeService));
        _openAIEmbeddingsService = openAIEmbeddingsService ?? throw new ArgumentNullException(nameof(openAIEmbeddingsService));
    }

    public async Task UpsertAsync(IList<EmbeddingsVectorUpsertModel> vectorUpsertModels, CancellationToken cancellationToken)
    {
        if(vectorUpsertModels.Count == 0)
        {
            return;
        }

        var vectors = await BuildVectorByNamespaceDictionaryAsync(vectorUpsertModels, cancellationToken);

        var upsertModels = CreateVectorIndexUpsertModel(vectors);

        foreach (var model in upsertModels)
        {
            await _pineconeService.UpsertAsync(model, cancellationToken);
        }
    }

    private async Task<IDictionary<string, IList<Vector>>> BuildVectorByNamespaceDictionaryAsync(IList<EmbeddingsVectorUpsertModel> vectorUpsertModels, CancellationToken cancellationToken)
    {
        var vectors = new Dictionary<string, IList<Vector>>();
        var vectorBuilder = new PineconeVectorBuilder();

        foreach (var model in vectorUpsertModels.GroupBy(v => v.Namespace))
        {
            vectors.Add(model.Key, new List<Vector>());

            foreach (var item in model)
            {
                var embeddings = await CreateEmbeddingsVectorAsync(new List<string> { item.Text }, cancellationToken);

                var vector = vectorBuilder.NewWith(embeddings, item.Metadata).Build();

                vectors[model.Key].Add(vector);
            }
        }

        return vectors;
    }

    private async Task<JArray> CreateEmbeddingsVectorAsync(IList<string> text, CancellationToken cancellationToken)
    {
        var createEmbeddingsModel = new CreateEmbeddingsModel
        {
            Input = text
        };

        var embeddings = await _openAIEmbeddingsService.CreateEmbeddingsAsync(createEmbeddingsModel, cancellationToken);
        return embeddings!.Data[0].EmbeddingVector;
    }

    private static UpsertDataModel[] CreateVectorIndexUpsertModel(IDictionary<string, IList<Vector>> vectorsByNamespace)
    {
        var models = new List<UpsertDataModel>();

        foreach (var vecNamespace in vectorsByNamespace)
        {
            var upsertDataModel = new UpsertDataModel
            {
                Namespace = vecNamespace.Key,
                Vectors = vecNamespace.Value.ToList()
            };

            models.Add(upsertDataModel);
        }

        return models.ToArray();
    }

    public async Task<IList<VectorQueryMatch>> QueryAsync(VectorQuery query, CancellationToken cancellationToken)
    {
        if(query == null)
        {
            return new List<VectorQueryMatch>();
        }

        var vectorBuilder = new PineconeVectorBuilder();

        var embeddings = await CreateEmbeddingsVectorAsync(new List<string>(query.Queries), cancellationToken);

        var vector = vectorBuilder.NewWith(embeddings).Build();

        var requestModel = new QueryVectorsModel
        {
            IncludeMetadata = true,
            IncludeValues = true,
            Namespace = query.Namespace,
            TopK = query.TopK,
            Vector = vector.Values
        };

        var queryResult = await _pineconeService.QueryVectorsAsync(requestModel, cancellationToken);

        var result = queryResult!.Matches.Select(match => new VectorQueryMatch
        {
            Id = match.Id,
            Metadata = match.Metadata,
            Score = match.Score,
            Values = match.Values
        }).ToList();

        return result;
    }
}
