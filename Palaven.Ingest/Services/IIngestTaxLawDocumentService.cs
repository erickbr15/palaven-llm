using Liara.Common;
using Palaven.Model.Ingest;

namespace Palaven.Ingest.Services;

public interface IIngestTaxLawDocumentService
{
    Task<IResult<TaxLawDocumentIngestTask>> IngestTaxLawDocumentAsync(IngestTaxLawDocumentCommand command, CancellationToken cancellationToken);
    Task CreateGoldenDocumentBatchAsync(Guid traceId, Guid lawId, int batchSize, CancellationToken cancellationToken);
    Task DeleteGoldenDocumentsAsync(Guid traceId, Guid lawId, CancellationToken cancellationToken);
}
