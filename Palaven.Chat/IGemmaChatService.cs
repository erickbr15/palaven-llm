using Palaven.Model.Chat;

namespace Palaven.Chat;

public interface IGemmaChatService
{
    ChatMessage CreateSimpleQueryPrompt(ChatMessage message);
    Task<ChatMessage> CreateAugmentedQueryPromptAsync(CreateAugmentedQueryPromptCommand command, CancellationToken cancellationToken);
}
