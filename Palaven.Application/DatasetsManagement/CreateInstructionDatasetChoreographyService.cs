using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.DatasetsManagement;

public class CreateInstructionDatasetChoreographyService : ICreateInstructionDatasetChoreographyService
{
    private readonly ICommandHandler<CreateInstructionDatasetCommand> _commandHandler;
    private readonly IMessageQueueService _messageQueueService;    

    public CreateInstructionDatasetChoreographyService(ICommandHandler<CreateInstructionDatasetCommand> commandHandler, IMessageQueueService messageQueueService)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
    }

    public async Task<IResult> CreateInstructionDatasetAsync(Message<CreateInstructionDatasetMessage> message, CancellationToken cancellationToken)
    {
        var command = new CreateInstructionDatasetCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(s => new Guid(s)).ToArray()
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            return result;
        }

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return result;
    }
}
