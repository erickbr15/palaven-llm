using Palaven.Model.Chat;

namespace Palaven.Chat.Contracts;

public interface IGemmaChatService
{
    ChatMessage GenerateSimpleQueryPrompt(ChatMessage message);
    Task<ChatMessage> AugmentQueryAsync(ChatMessage message, CancellationToken cancellationToken);
}
