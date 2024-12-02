using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Application.Abstractions.VectorIndexing;
using Palaven.Application.Model.VectorIndexing;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.VectorIndexing.Services;

public class InstructionsIndexingChoreographyService : IInstructionsIndexingChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult> _commandHandler;
    private readonly INotificationService _notificationService;

    public InstructionsIndexingChoreographyService(IMessageQueueService messageQueueService, ICommandHandler<IndexInstructionsCommand, InstructionsIndexingResult> commandHandler,
        INotificationService notificationService)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<IResult<InstructionsIndexingResult>> IndexInstructionsAsync(Message<IndexInstructionsMessage> message, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.Indexing.NotificationVectorIndexingInvoked, message.Body.OperationId, message.MessageId), cancellationToken);

        var command = new IndexInstructionsCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(d=>new Guid(d)).ToArray()
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);

        if(!result.HasErrors)
        {
            await _notificationService.SendAsync(new Guid(message.Body.TenantId),
                string.Format(Resources.Indexing.NotificationVectorIndexingSuccess, message.Body.OperationId, message.MessageId), cancellationToken);

            await _messageQueueService.DeleteMessageAsync(message, cancellationToken);
        }

        return result;
    }   
}
