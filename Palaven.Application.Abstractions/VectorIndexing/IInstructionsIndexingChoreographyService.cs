using Liara.Common.Abstractions;
using Palaven.Application.Model.VectorIndexing;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Abstractions.VectorIndexing;

public interface IInstructionsIndexingChoreographyService
{
    Task<IResult<InstructionsIndexingResult>> IndexInstructionsAsync(Message<IndexInstructionsMessage> message, CancellationToken cancellationToken);
}
