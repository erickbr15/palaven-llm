using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.VectorIndexing;
using Palaven.Application.Model.VectorIndexing;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.VectorIndexing.Services;

public class InstructionsIndexingChoreographyService : IInstructionsIndexingChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult> _commandHandler;

    public InstructionsIndexingChoreographyService(IMessageQueueService messageQueueService, ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult> commandHandler)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    public async Task<IResult<InstructionsIndexingResult>> IndexInstructionsAsync(Message<IndexInstructionsMessage> message, CancellationToken cancellationToken)
    {
        var messageBody = message.Body;

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        var command = new IndexInstructionsCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(d=>new Guid(d)).ToArray()
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);

        if(result.HasErrors)
        {
            await _messageQueueService.SendMessageAsync(messageBody, cancellationToken);
            return result;
        }

        return result;
    }   
}
