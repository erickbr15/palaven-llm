using Palaven.Model.Chat;

namespace Palaven.Chat.Contracts;

public interface IGemmaChatService
{
    Task<ChatMessage> AugmentQueryAsync(ChatMessage message, CancellationToken cancellationToken);
}
