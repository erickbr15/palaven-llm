using Azure.Storage.Queues;
using Liara.Integrations.Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.MicrosoftAzure.Storage;

namespace Palaven.Infrastructure.MicrosoftAzure.Messaging;

public class QueueClientProvider : IQueueClientProvider
{
    private readonly Lazy<IDictionary<Type, QueueClient>> _queueClients;
    private readonly StorageAccountOptions _storageAccountOptions;
    private readonly QueueServiceClient _queueServiceClient;

    public QueueClientProvider(IOptions<StorageAccountOptions> optionsService, IAzureClientFactory<QueueServiceClient> azureClientFactory)
    {
        _storageAccountOptions = optionsService?.Value ?? throw new InvalidOperationException("Unable to get StorageAccountOptions");
        _queueServiceClient = azureClientFactory.CreateClient("AzureStorageLawDocs") ?? throw new InvalidOperationException("Unable to create QueueServiceClient");
        
        _queueClients = new Lazy<IDictionary<Type, QueueClient>>(() => new Dictionary<Type, QueueClient>());
    }

    public QueueClient GetQueueClient(Type messageType)
    {
        var queueConfigName = QueueStorageConfigNames.GetQueueConfigNameByMessageType(messageType);
        if(!_queueClients.Value.ContainsKey(messageType))
        {
            var queueName = _storageAccountOptions.QueueNames[queueConfigName];
            _queueClients.Value.Add(messageType, _queueServiceClient.GetQueueClient(queueName));
        }

        return _queueClients.Value[messageType];
    }
}
