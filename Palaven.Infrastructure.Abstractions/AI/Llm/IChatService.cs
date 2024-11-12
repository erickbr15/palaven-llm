namespace Palaven.Infrastructure.Abstractions.AI.Llm;

public interface IChatService
{
    Task<string> GetChatResponseAsync(string query, CancellationToken cancellationToken);
}
