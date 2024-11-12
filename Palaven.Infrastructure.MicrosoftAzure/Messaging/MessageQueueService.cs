using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;
using System.Text.Json;

namespace Palaven.Infrastructure.MicrosoftAzure.Messaging;

public class MessageQueueService : IMessageQueueService
{
    private readonly IQueueClientProvider _queueClientProvider;

    public MessageQueueService(IQueueClientProvider queueClientProvider)
    {
        _queueClientProvider = queueClientProvider ?? throw new ArgumentNullException(nameof(queueClientProvider));
    }

    public async Task DeleteMessageAsync<TBody>(Message<TBody> message, CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));
        await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);        
    }    

    public async Task<Message<TBody>> PeekMessageAsync<TBody>(CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.PeekMessageAsync(cancellationToken);

        var body = JsonSerializer.Deserialize<TBody>(azureResponse.Value.Body.ToString());

        var message = new Message<TBody>
        {
            Body = body!,
            MessageId = azureResponse.Value.MessageId,            
            InsertedOn = azureResponse.Value.InsertedOn,
            ExpiresOn = azureResponse.Value.ExpiresOn,
            DequeueCount = azureResponse.Value.DequeueCount
        };

        return message;
    }

    public async Task<IEnumerable<TBody>> PeekMessagesAsync<TBody>(int? maxMessages, CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.PeekMessagesAsync(maxMessages, cancellationToken);

        var messages = azureResponse.Value.Select(azureMessage =>
        {
            var body = JsonSerializer.Deserialize<TBody>(azureMessage.Body.ToString());
            return body!;
        });

        return messages;
    }

    public async Task<Message<TBody>> ReceiveMessageAsync<TBody>(TimeSpan? visibilityTimeout = null,CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.ReceiveMessageAsync(visibilityTimeout, cancellationToken);

        var body = JsonSerializer.Deserialize<TBody>(azureResponse.Value.Body.ToString());

        var message = new Message<TBody>
        {
            Body = body!,
            MessageId = azureResponse.Value.MessageId,
            PopReceipt = azureResponse.Value.PopReceipt,
            NextVisibleOn = azureResponse.Value.NextVisibleOn,
            InsertedOn = azureResponse.Value.InsertedOn,
            ExpiresOn = azureResponse.Value.ExpiresOn,
            DequeueCount = azureResponse.Value.DequeueCount
        };

        return message;
    }

    public async Task<IEnumerable<TBody>> ReceiveMessagesAsync<TBody>(TimeSpan? visibilityTimeout = null, int? maxMessages = null, CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.ReceiveMessagesAsync(maxMessages, visibilityTimeout, cancellationToken);

        var messages = azureResponse.Value.Select(azureMessage =>
        {
            var body = JsonSerializer.Deserialize<TBody>(azureMessage.Body.ToString());
            return body!;
        });

        return messages;
    }

    public async Task SendMessageAsync<TBody>(TBody messageBody, CancellationToken cancellationToken) where TBody : class
    {
        if(messageBody is null)
        {
            return;
        }

        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));
        var messageJson = JsonSerializer.Serialize(messageBody);

        await queueClient.SendMessageAsync(messageJson, cancellationToken);          
    }
}
