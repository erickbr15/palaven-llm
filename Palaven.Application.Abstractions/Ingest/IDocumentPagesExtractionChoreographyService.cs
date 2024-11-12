using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Abstractions.Ingest;

public interface IDocumentPagesExtractionChoreographyService
{
    Task<IResult<EtlTaskDocument>> ExtractPagesAsync(Message<ExtractDocumentPagesMessage> message, CancellationToken cancellationToken);
}
