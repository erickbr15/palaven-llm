using Liara.Common;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;

namespace Palaven.Ingest;

public interface IIngestTaxLawDocumentService
{
    Task<IResult<EtlTaskDocument>> StartTaxLawIngestAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken);
    Task<IResult<EtlTaskDocument>> IngestTaxLawDocumentAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken);
    Task CreateGoldenDocumentBatchAsync(Guid traceId, Guid lawId, int batchSize, CancellationToken cancellationToken);
    Task DeleteGoldenDocumentsAsync(Guid traceId, Guid lawId, CancellationToken cancellationToken);
}
