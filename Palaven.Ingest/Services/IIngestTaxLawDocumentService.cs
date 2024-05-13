using Liara.Common;
using Palaven.Model.Ingest.Commands;

namespace Palaven.Ingest.Services;

public interface IIngestTaxLawDocumentService
{
    Task<IResult<IngestLawDocumentTaskInfo>> IngestTaxLawDocumentAsync(IngestLawDocumentModel model, CancellationToken cancellationToken);
    Task CreateGoldenDocumentsAsync(Guid traceId, Guid lawId, int chunkSize, CancellationToken cancellationToken);
}
