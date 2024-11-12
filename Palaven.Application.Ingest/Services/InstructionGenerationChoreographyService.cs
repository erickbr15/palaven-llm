using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Ingest.Services;

public class InstructionGenerationChoreographyService : IInstructionGenerationChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> _generateInstructionsCommandHandler;

    public InstructionGenerationChoreographyService(IMessageQueueService messageQueueService, 
        ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> commandHandler)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _generateInstructionsCommandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    public async Task<IResult<InstructionGenerationResult>> GenerateInstructionsAsync(Message<GenerateInstructionsMessage> message, CancellationToken cancellationToken)
    {
        var command = new GenerateInstructionsCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(d=> new Guid(d)).ToArray()
        };

        var result = await _generateInstructionsCommandHandler.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            return result;
        }

        var indexMessage = new IndexInstructionsMessage
        {
            OperationId = message.Body.OperationId,
            DocumentIds = result.Value.SuccessfulDocumentIds.Select(d => d.ToString()).ToArray()
        };

        await _messageQueueService.SendMessageAsync(indexMessage, cancellationToken);
        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return result;
    }
}