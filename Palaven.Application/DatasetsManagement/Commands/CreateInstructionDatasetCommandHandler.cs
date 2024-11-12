using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Persistence.Abstractions;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Application.DatasetManagement.Commands;

public class CreateInstructionDatasetCommandHandler : ICommandHandler<CreateInstructionDatasetCommand>
{
    private readonly IDocumentRepository<GoldenDocument> _documentRepository;
    private readonly IDatasetsDataService _datasetsDataService;

    public CreateInstructionDatasetCommandHandler(IDocumentRepository<GoldenDocument> documentRepository,
        IDatasetsDataService datasetsDataService)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        _datasetsDataService = datasetsDataService ?? throw new ArgumentNullException(nameof(datasetsDataService));
    }    

    public async Task<IResult> ExecuteAsync(CreateInstructionDatasetCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return Result.Fail(new ArgumentNullException(nameof(command)));
        }

        var documents = await FetchDocumentsAsync(command.DocumentIds, cancellationToken);
        if(documents == null || !documents.Any())
        {
            return Result.Fail(new InvalidOperationException("No documents found"));
        }

        foreach (var document in documents)
        {
            await CleanupDocumentInstructionsAsync(document.Id, cancellationToken);

            foreach (var instruction in document.Instructions)
            {
                var instructionEntity = new InstructionEntity
                {
                    InstructionId = instruction.InstructionId,
                    DatasetId = command.OperationId,
                    Instruction = instruction.InstructionText,
                    Response = instruction.Response,
                    Category = instruction.Type,
                    GoldenArticleId = document.Id                    
                };

                await _datasetsDataService.CreateAsync(instructionEntity, cancellationToken);
            }
        }

        await _datasetsDataService.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task CleanupDocumentInstructionsAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var documentInstructionIds = _datasetsDataService.GetInstructionQueryable()
            .Where(i => i.GoldenArticleId == documentId)
            .Select(i => i.Id)
            .ToList();

        foreach (var instructionId in documentInstructionIds)
        {
            await _datasetsDataService.DeleteInstructionAsync(instructionId, cancellationToken);
        }
    }

    private async Task<IEnumerable<GoldenDocument>> FetchDocumentsAsync(Guid[] documentIds, CancellationToken cancellationToken)
    {
        var queryText = $"SELECT * FROM c WHERE c.id IN ({string.Join(",", documentIds.Select(x => $"'{x}'"))})";
        var goldenArticles = await _documentRepository.GetAsync(queryText, continuationToken: null, cancellationToken: cancellationToken);

        return goldenArticles;
    }
}
