using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Abstractions.Ingest;

public interface IArticleParagraphsExtractionChoreographyService
{
    Task<IResult<EtlTaskDocument>> ExtractArticleParagraphsAsync(Message<ExtractArticleParagraphsMessage> message, CancellationToken cancellationToken);
}
