using Palaven.Model.Chat;

namespace Palaven.Chat.Contracts;

public interface IOpenAIChatService
{
    Task<string> GetChatResponseAsync(ChatMessage chatMessage, CancellationToken cancellationToken);
}
