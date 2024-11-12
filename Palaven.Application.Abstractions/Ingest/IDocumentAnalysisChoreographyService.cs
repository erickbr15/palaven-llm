using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Abstractions.Ingest;

public interface IDocumentAnalysisChoreographyService
{
    Task<IResult<string>> StartDocumentAnalysisAsync(Message<DocumentAnalysisMessage> message, CancellationToken cancellationToken);
}
