using Palaven.Model.Chat;

namespace Palaven.Chat.Contracts;

public interface IGemmaQueryAugmentationService
{
    Task<ChatMessage> AugmentQueryAsync(ChatMessage message, CancellationToken cancellationToken);
}
