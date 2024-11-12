using Liara.Common.Abstractions;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Abstractions.Ingest;

public interface IStartIngestionChoreographyService
{
    Task<IResult<EtlTaskDocument>> StartIngestionAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken);
}
