using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Common.Abstractions.Persistence;
using Palaven.Application.Ingest.Resources;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.AI;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Commands;

public class StartDocumentAnalysisCommandHandler : ICommandHandler<AnalyzeDocumentCommand, string>
{
    private readonly IPdfDocumentAnalyzer _documentAnalyzer;
    private readonly IDocumentRepository<EtlTaskDocument> _repository;

    public StartDocumentAnalysisCommandHandler(IPdfDocumentAnalyzer pdfDocumentAnalyzer, IDocumentRepository<EtlTaskDocument> taskDocumentRepository)
    {
        _documentAnalyzer = pdfDocumentAnalyzer ?? throw new ArgumentNullException(nameof(pdfDocumentAnalyzer));
        _repository = taskDocumentRepository ?? throw new ArgumentNullException(nameof(taskDocumentRepository));        
    }

    public async Task<IResult<string>> ExecuteAsync(AnalyzeDocumentCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<string>.Fail(new ArgumentNullException(nameof(command))));
        }

        var etlTask = await _repository.GetByIdAsync(command.OperationId.ToString(), cancellationToken);
        if (etlTask == null)
        {
            return await Task.FromResult(Result<string>.Fail(new InvalidOperationException($"Unable to find the ETL task document with id {command.OperationId}")));
        }

        var analysisOperationId = await _documentAnalyzer.AnalyzeDocumentAsync(command.DocumentContent, command.DocumentLocale, command.DocumentPages, cancellationToken);                
        etlTask.Details.Add($"{DateTime.UtcNow.ToString()}.{Etl.BronzeStageStartDocumentAnalysis}. OperationId: {analysisOperationId}");

        await _repository.UpsertAsync(etlTask, etlTask.UserId.ToString(), cancellationToken);

        return Result<string>.Success(analysisOperationId);
    }       
}
