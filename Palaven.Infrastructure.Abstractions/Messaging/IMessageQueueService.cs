using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Infrastructure.Abstractions.Messaging;

public interface IMessageQueueService
{
    Task DeleteMessageAsync<TBody>(Message<TBody> message, CancellationToken cancellationToken = default) where TBody : class;
    Task<Message<TBody>?> PeekMessageAsync<TBody>(CancellationToken cancellationToken = default) where TBody : class;
    Task<IEnumerable<TBody>> PeekMessagesAsync<TBody>(int? maxMessages, CancellationToken cancellationToken = default) where TBody : class;
    Task<Message<TBody>?> ReceiveMessageAsync<TBody>(TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) where TBody : class;
    Task<IEnumerable<TBody>> ReceiveMessagesAsync<TBody>(TimeSpan? visibilityTimeout = null, int? maxMessages = null, CancellationToken cancellationToken = default) where TBody : class;
    Task SendMessageAsync<TBody>(TBody messageBody, CancellationToken cancellationToken) where TBody : class;    
}
