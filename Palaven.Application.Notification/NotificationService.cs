using Liara.Persistence.Abstractions;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Notification;

public class NotificationService : INotificationService
{
    private readonly IDocumentRepository<NotificationDocument> _documentRepository;

    public NotificationService(IDocumentRepository<NotificationDocument> documentRepository)
    {
        _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public Task SendAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        var notificationDocument = new NotificationDocument
        {
            Id = Guid.NewGuid(),
            TenantId = userId,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        return _documentRepository.CreateAsync(notificationDocument, userId.ToString(), cancellationToken);
    }
}
