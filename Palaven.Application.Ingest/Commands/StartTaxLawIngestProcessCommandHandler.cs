using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Common.Abstractions.Persistence;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Storage;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Commands;

public class StartTaxLawIngestProcessCommandHandler : ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument>
{
    private readonly IEtlInboxStorage _etlInboxStorage;
    private readonly IDocumentRepository<EtlTaskDocument> _repository;    

    public StartTaxLawIngestProcessCommandHandler(IEtlInboxStorage etlInboxStorage, IDocumentRepository<EtlTaskDocument> repository)
    {
        _etlInboxStorage = etlInboxStorage ?? throw new ArgumentNullException(nameof(etlInboxStorage));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IResult<EtlTaskDocument>> ExecuteAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            return await Task.FromResult(Result<EtlTaskDocument>.Fail(new List<ValidationError>(), new List<Exception> { new ArgumentNullException(nameof(command)) }));
        }

        var etlTask = command.ToEtlTaskDocument();

        await _repository.CreateAsync(etlTask, etlTask.UserId.ToString(), cancellationToken);
        await _etlInboxStorage.AppendAsync(etlTask.Metadata[EtlMetadataKeys.FileName], command.FileContent, etlTask.Metadata, cancellationToken);        

        return new Result<EtlTaskDocument> { Value = etlTask };
    }
}