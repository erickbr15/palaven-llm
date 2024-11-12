using Liara.Common.Abstractions;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Abstractions.Ingest;

public interface IArticlesCurationChoreographyService
{
    Task<IResult> CurateArticlesAsync(Message<CurateArticlesMessage> message, CancellationToken cancellationToken);
}
