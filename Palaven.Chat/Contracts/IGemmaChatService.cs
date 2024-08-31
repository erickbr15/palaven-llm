using Palaven.Model.Chat;

namespace Palaven.Chat.Contracts;

public interface IGemmaChatService
{
    ChatMessage CreateSimpleQueryPrompt(ChatMessage message);
    Task<ChatMessage> CreateAugmentedQueryPromptAsync(ChatMessage message, CancellationToken cancellationToken);
}
