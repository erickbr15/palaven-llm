using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;
using System.Net;
using System.Text;
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

    public async Task<Message<TBody>?> PeekMessageAsync<TBody>(CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.PeekMessageAsync(cancellationToken); 
        
        if(azureResponse.GetRawResponse().IsError || 
            azureResponse.GetRawResponse().Status == (int)HttpStatusCode.NoContent ||
            azureResponse.Value is null)
        {
            return null;
        }

        var decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(azureResponse.Value.Body.ToString()));
        
        var body = JsonSerializer.Deserialize<TBody>(decodedMessage);

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
            var decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(azureMessage.Body.ToString()));
            var body = JsonSerializer.Deserialize<TBody>(decodedMessage);
            return body!;
        });

        return messages;
    }

    public async Task<Message<TBody>?> ReceiveMessageAsync<TBody>(TimeSpan? visibilityTimeout = null,CancellationToken cancellationToken = default) where TBody : class
    {
        var queueClient = _queueClientProvider.GetQueueClient(typeof(TBody));

        var azureResponse = await queueClient.ReceiveMessageAsync(visibilityTimeout, cancellationToken);

        if (azureResponse.GetRawResponse().IsError || 
            azureResponse.GetRawResponse().Status == (int)HttpStatusCode.NoContent ||
            azureResponse.Value is null)
        {
            return null;
        }

        var decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(azureResponse.Value.Body.ToString()));

        var body = JsonSerializer.Deserialize<TBody>(decodedMessage);

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
            var decodedMessage = Encoding.UTF8.GetString(Convert.FromBase64String(azureMessage.Body.ToString()));
            var body = JsonSerializer.Deserialize<TBody>(decodedMessage);
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
        var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(messageJson));

        await queueClient.SendMessageAsync(encodedMessage, cancellationToken);
    }
}
