namespace Palaven.Infrastructure.Abstractions.Messaging;

public interface IMessageSender
{
    Task BroadcastAsync(string target, object message, CancellationToken cancellationToken = default);
    Task SendToUserAsync(string userId, string target, object message, CancellationToken cancellationToken = default);
    Task SendToGroupAsync(string groupName, string target, object message, CancellationToken cancellationToken = default);
}
