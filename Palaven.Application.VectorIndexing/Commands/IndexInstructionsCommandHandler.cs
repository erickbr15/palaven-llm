using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Common.Abstractions.Persistence;
using Palaven.Application.Model.VectorIndexing;
using Palaven.Infrastructure.Abstractions.VectorIndexing;
using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Model.Persistence.Documents.Metadata;

namespace Palaven.VectorIndexing.Commands;

public class IndexInstructionsCommandHandler : ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult>
{
    private readonly IVectorIndexService _vectorIndexingService;
    private readonly IDocumentRepository<GoldenDocument> _goldenArticleRepository;
    
    public IndexInstructionsCommandHandler(IVectorIndexService vectorIndexingService, IDocumentRepository<GoldenDocument> goldenArticleRepository)
    {
        _vectorIndexingService = vectorIndexingService ?? throw new ArgumentNullException(nameof(vectorIndexingService));
        _goldenArticleRepository = goldenArticleRepository ?? throw new ArgumentNullException(nameof(goldenArticleRepository));
    }
    
    public async Task<IResult<InstructionsIndexingResult>> ExecuteAsync(IndexInstructionsCommand command, CancellationToken cancellationToken)
    {        
        var goldenDocuments = await FetchGoldenDocumentsAsync(command, cancellationToken);
        var indexingResult = new InstructionsIndexingResult { OperationId = command.OperationId };
        var exceptions = new List<Exception>();

        foreach (var goldenDocument in goldenDocuments)
        {
            try
            {                
                var vectorUpsertModels = CreateEmbeddingsVectorUpsertModels(goldenDocument, "palaven", goldenDocument.Instructions);

                await _vectorIndexingService.UpsertAsync(vectorUpsertModels, cancellationToken);

                indexingResult.SucessfulDocumentIds.Add(goldenDocument.Id);
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
                indexingResult.FailedDocumentIds.Add(goldenDocument.Id);
            }
        }

        if (exceptions.Any())
        {
            var result = Result<InstructionsIndexingResult>.Fail(exceptions);
            result.Value = indexingResult;
            return result;
        }

        return Result<InstructionsIndexingResult>.Success(indexingResult);
    }

    private Task<IEnumerable<GoldenDocument>> FetchGoldenDocumentsAsync(IndexInstructionsCommand command, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM c WHERE c.trace_id='{command.OperationId}' AND c.id IN ({string.Join(",", command.DocumentIds.Select(d => $"'{d}'"))})";
        var goldenDocumentsQuery = _goldenArticleRepository.GetAsync(query, continuationToken: null, cancellationToken);

        return goldenDocumentsQuery;
    }

    private static IList<EmbeddingsVectorUpsertModel> CreateEmbeddingsVectorUpsertModels(GoldenDocument document, string vectorNamespace, IEnumerable<Instruction> instructions)
    {
        var models = new List<EmbeddingsVectorUpsertModel>();

        foreach (var instruction in instructions)
        {
            var embeddingsVectorUpsertModel = new EmbeddingsVectorUpsertModel
            {
                Namespace = vectorNamespace,
                Text = instruction.InstructionText
            };
            
            document.Metadata.ToList().ForEach(kv => embeddingsVectorUpsertModel.Metadata.Add(kv.Key, kv.Value));

            embeddingsVectorUpsertModel.Metadata.Add("GoldenDocumentId", document.Id);
            embeddingsVectorUpsertModel.Metadata.Add("ArticleLawId", document.ArticleId);

            models.Add(embeddingsVectorUpsertModel);
        }

        return models;
    }
}