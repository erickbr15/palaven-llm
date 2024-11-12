using Liara.Persistence.Abstractions;
using Microsoft.Extensions.Options;
using Palaven.Infrastructure.Abstractions.AI.Llm;
using Palaven.Infrastructure.Abstractions.VectorIndexing;
using Palaven.Infrastructure.Model.AI.Llm;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Infrastructure.Llm;

public class PalavenRetrievalService : IRetrievalService
{    
    private readonly IVectorIndexService _vectorIndexService;
    private readonly IDocumentRepository<GoldenDocument> _documentsRepository;

    public PalavenRetrievalService(IVectorIndexService vectorIndexService, IDocumentRepository<GoldenDocument> documentsRepository)
    {
        _vectorIndexService = vectorIndexService ?? throw new ArgumentNullException(nameof(vectorIndexService));
        _documentsRepository = documentsRepository ?? throw new ArgumentNullException(nameof(documentsRepository));
    }

    public async Task<IEnumerable<TDocument>> RetrieveRelatedDocumentsAsync<TDocument>(IEnumerable<string> instructions, RetrievalOptions options, CancellationToken cancellationToken) where TDocument : class
    {
        if(instructions == null || !instructions.Any())
        {
            return new List<TDocument>();
        }

        var queryVectorResult = await _vectorIndexService.QueryAsync(new Model.VectorIndexing.VectorQuery
        {
            Namespace = options.Namespace!,
            TopK = options.TopK,
            Queries = new List<string>(instructions)
        }, cancellationToken);        


        if (queryVectorResult == null || !queryVectorResult.Any(match => match.Score >= options.MinimumMatchScore))
        {
            return new List<TDocument>();
        }

        var articleIds = queryVectorResult
            .SelectMany(m => m.Metadata)
            .Where(m => m.Key == "golden_article_id")
            .Select(m => m.Value.ToString())
            .Distinct()
            .ToList();

        var documentQuery = $"SELECT * FROM c WHERE c.id IN ({string.Join(",", articleIds.Select(a => $"'{a}'"))})";

        var goldenArticles = (await _documentsRepository.GetAsync(documentQuery, continuationToken: null, cancellationToken: cancellationToken)) ??
            new List<GoldenDocument>();

        return (IEnumerable<TDocument>)goldenArticles;
    }

    

    
}
