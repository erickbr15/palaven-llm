using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Persistence.Abstractions;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.AI;
using Palaven.Infrastructure.Model.AI;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Commands;

public class GetDocumentAnalysisResultCommandHandler : ICommandHandler<GetDocumentAnalysisResultCommand, DocumentAnalysisResult>
{
    private readonly IPdfDocumentAnalyzer _pdfDocumentAnalyzer;
    private readonly IDocumentRepository<EtlTaskDocument> _etlTaskRepository;

    public GetDocumentAnalysisResultCommandHandler(IPdfDocumentAnalyzer pdfDocumentAnalyzer, IDocumentRepository<EtlTaskDocument> documentRepository)
    {
        _pdfDocumentAnalyzer = pdfDocumentAnalyzer ?? throw new ArgumentNullException(nameof(pdfDocumentAnalyzer));
        _etlTaskRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task<IResult<DocumentAnalysisResult>> ExecuteAsync(GetDocumentAnalysisResultCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return Result<DocumentAnalysisResult>.Fail(new ArgumentNullException(nameof(command)));
        }

        var etlTask = await _etlTaskRepository.GetByIdAsync(command.OperationId.ToString(), cancellationToken);
        
        var documentAnalysisResult = await _pdfDocumentAnalyzer.GetDocumentAnalysisResultAsync(command.DocumentAnalysisOperationId, cancellationToken);
        if (documentAnalysisResult.HasErrors)
        {
            etlTask.Details.Add($"{DateTime.UtcNow.ToString()}. Error getting document analysis result. OperationId: {command.DocumentAnalysisOperationId}");
            await _etlTaskRepository.UpsertAsync(etlTask!, etlTask!.UserId.ToString(), cancellationToken);

            return documentAnalysisResult;
        }

        etlTask.Details.Add($"{DateTime.UtcNow.ToString()}. Document analysis result received. OperationId: {command.DocumentAnalysisOperationId}. IsComplete: {documentAnalysisResult.Value.IsComplete}");        
        await _etlTaskRepository.UpsertAsync(etlTask!, etlTask!.UserId.ToString(), cancellationToken);

        documentAnalysisResult.Value.EtlRelatedTask = etlTask;

        return documentAnalysisResult;        
    }
}
