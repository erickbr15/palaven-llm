using Azure.Storage.Queues;

namespace Palaven.Infrastructure.Abstractions.Messaging;

public interface IQueueClientProvider
{
    public QueueClient GetQueueClient(Type messageType);
}
