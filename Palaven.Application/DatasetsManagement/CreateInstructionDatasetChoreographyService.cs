using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.DatasetsManagement;

public class CreateInstructionDatasetChoreographyService : ICreateInstructionDatasetChoreographyService
{
    private readonly ICommandHandler<CreateInstructionDatasetCommand> _commandHandler;
    private readonly IMessageQueueService _messageQueueService;
    private readonly INotificationService _notificationService;

    public CreateInstructionDatasetChoreographyService(ICommandHandler<CreateInstructionDatasetCommand> commandHandler, IMessageQueueService messageQueueService,
        INotificationService notificationService)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<IResult> CreateInstructionDatasetAsync(Message<CreateInstructionDatasetMessage> message, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.DatasetManagement.NotificationCreateInstructionDatasetInvoked, message.Body.OperationId, message.MessageId), cancellationToken);

        var command = new CreateInstructionDatasetCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(s => new Guid(s)).ToArray()
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            await _notificationService.SendAsync(new Guid(message.Body.TenantId), 
                string.Format(Resources.DatasetManagement.NotificationCreateInstructionDatasetError,message.Body.OperationId, message.MessageId), cancellationToken);

            return result;
        }

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.DatasetManagement.NotificationCreateInstructionDatasetSuccess, message.Body.OperationId, message.MessageId), cancellationToken);

        return result;
    }
}
