using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Notifications.FunctionApp;

public class NotificationSenderFunction
{
    private readonly ILogger _logger;
    private readonly IMessageSender _messageSender;

    public NotificationSenderFunction(ILoggerFactory loggerFactory, IMessageSender messageSender)
    {
        _logger = loggerFactory.CreateLogger<NotificationSenderFunction>();
        _messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
    }

    [Function(nameof(NotificationSenderFunction))]
    public void RunAsync([CosmosDBTrigger(
        databaseName: "palaven-satdocs",
        containerName: "notifications",
        Connection = "PalavenCosmosDB", 
        CreateLeaseContainerIfNotExists = false)] IReadOnlyList<NotificationDocument> notifications, CancellationToken cancellationToken)
    {
        if (notifications is null || !notifications.Any())
        {
            _logger.LogInformation("No notifications to send.");
            return;
        }

        foreach (var notification in notifications)
        {
            _logger.LogInformation($"Sending notification: {notification.Message}");            
            _messageSender.SendToUserAsync(notification.TenantId.ToString(), notification.TenantId.ToString(), notification.Message, cancellationToken);            
        }
    }
}
