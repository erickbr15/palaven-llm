using Liara.Common.Abstractions;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Abstractions.Ingest;

public interface IInstructionGenerationChoreographyService
{
    Task<IResult<InstructionGenerationResult>> GenerateInstructionsAsync(Message<GenerateInstructionsMessage> message, CancellationToken cancellationToken);
}
