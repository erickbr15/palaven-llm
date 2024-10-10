using Palaven.Model.Chat;

namespace Palaven.Chat;

public interface IOpenAIChatService
{
    Task<string> GetChatResponseAsync(ChatMessage chatMessage, CancellationToken cancellationToken);
}
