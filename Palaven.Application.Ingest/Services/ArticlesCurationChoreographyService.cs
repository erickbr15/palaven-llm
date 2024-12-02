using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Ingest.Services;

public class ArticlesCurationChoreographyService : IArticlesCurationChoreographyService
{
    private readonly ICommandHandler<CurateArticlesCommand, ArticlesCurationResult> _curateArticlesCommandHandler;
    private readonly IMessageQueueService _messageQueueService;
    private readonly INotificationService _notificationService;

    public ArticlesCurationChoreographyService(ICommandHandler<CurateArticlesCommand, ArticlesCurationResult> curateArticlesCommandHandler, IMessageQueueService messageQueueService,
        INotificationService notificationService)
    {
        _curateArticlesCommandHandler = curateArticlesCommandHandler ?? throw new ArgumentNullException(nameof(curateArticlesCommandHandler));
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }    

    public async Task<IResult> CurateArticlesAsync(Message<CurateArticlesMessage> message, CancellationToken cancellationToken)
    {                
        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.Etl.NotificationArticleCurationInvoked, message.Body.OperationId, message.MessageId), cancellationToken);

        var command = new CurateArticlesCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(s=>new Guid(s)).ToArray()
        };

        var result = await _curateArticlesCommandHandler.ExecuteAsync(command, cancellationToken);
        if(result.HasErrors)
        {
            await _notificationService.SendAsync(new Guid(message.Body.TenantId), 
                string.Format(string.Format(Resources.Etl.NotificacionEtlError, message.Body.OperationId)), cancellationToken);

            return result;
        }

        var generateInstructionsMessage = new GenerateInstructionsMessage
        {
            OperationId = result.Value.OperationId.ToString(),
            TenantId = message.Body.TenantId,
            DocumentIds = result.Value.CuratedDocumentIds.Select(s => s.ToString()).ToArray()
        };

        await _messageQueueService.SendMessageAsync(generateInstructionsMessage, cancellationToken);
        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        await _notificationService.SendAsync(new Guid(message.Body.TenantId),
            string.Format(Resources.Etl.NotificationArticleCurationSuccess, message.Body.OperationId, message.MessageId), cancellationToken);

        return Result.Success();
    }
}
