using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Ingest.Services;

public class InstructionGenerationChoreographyService : IInstructionGenerationChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> _generateInstructionsCommandHandler;
    private readonly INotificationService _notificationService;

    public InstructionGenerationChoreographyService(IMessageQueueService messageQueueService, ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> commandHandler,
        INotificationService notificationService)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _generateInstructionsCommandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<IResult<InstructionGenerationResult>> GenerateInstructionsAsync(Message<GenerateInstructionsMessage> message, CancellationToken cancellationToken)
    {
        await _notificationService.SendAsync(new Guid(message.Body.TenantId), 
            string.Format(Resources.Etl.NotificationGenerateInstructionsInvoked, message.Body.OperationId, message.MessageId), cancellationToken);

        var command = new GenerateInstructionsCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(d=> new Guid(d)).ToArray()
        };

        var result = await _generateInstructionsCommandHandler.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            await _notificationService.SendAsync(new Guid(message.Body.TenantId),
                string.Format(Resources.Etl.NotificacionEtlError, message.Body.OperationId), cancellationToken);

            return result;
        }

        var indexMessage = new IndexInstructionsMessage
        {
            OperationId = message.Body.OperationId,
            TenantId = message.Body.TenantId,
            DocumentIds = result.Value.SuccessfulDocumentIds.Select(d => d.ToString()).ToArray()
        };
        await _messageQueueService.SendMessageAsync(indexMessage, cancellationToken);

        var instructionDatasetMessage = new CreateInstructionDatasetMessage
        {
            OperationId = message.Body.OperationId,
            TenantId = message.Body.TenantId,
            DocumentIds = result.Value.SuccessfulDocumentIds.Select(d => d.ToString()).ToArray()
        };
        await _messageQueueService.SendMessageAsync(instructionDatasetMessage, cancellationToken);

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.Etl.NotificationGenerateInstructionsSuccess, message.Body.OperationId, message.MessageId), cancellationToken);

        return result;
    }
}