using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Liara.Azure.Storage;
using Liara.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;

namespace Palaven.Etl.FunctionApp;

public class CreateSilverDocumentFunction
{
    private readonly ILogger<CreateSilverDocumentFunction> _logger;
    private readonly QueueClient _silverStageQueue;
    private readonly QueueClient _goldenStageQueue;
    private readonly ICommandHandler<CreateSilverDocumentCommand, EtlTaskDocument> _commandHandler;

    public CreateSilverDocumentFunction(ILogger<CreateSilverDocumentFunction> logger, IOptions<AzureStorageOptions> storageOptionsService,
        IAzureClientFactory<QueueServiceClient> azureQueueClientFactory,
        ICommandHandler<CreateSilverDocumentCommand, EtlTaskDocument> commandHandler)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var storageOptions = storageOptionsService.Value ?? throw new ArgumentNullException(nameof(storageOptionsService));
        
        _silverStageQueue = azureQueueClientFactory.CreateClient("AzureStorageLawDocs")
            .GetQueueClient(storageOptions.QueueNames[QueueStorageNames.SilverStageQueue]);

        _goldenStageQueue = azureQueueClientFactory.CreateClient("AzureStorageLawDocs")
            .GetQueueClient(storageOptions.QueueNames[QueueStorageNames.GoldStageQueue]);

        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    [Function(nameof(CreateSilverDocumentFunction))]
    public async Task RunAsync([QueueTrigger("silver-stage-queue", Connection = "AzureStorageLawDocs")] QueueMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

        if (!TryToDeserializeMessage(message.MessageText, out var silverStageMessage))
        {
            await _silverStageQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
            _logger.LogError("Failed to deserialize message");
            return;
        }

        var command = new CreateSilverDocumentCommand
        {
            OperationId = new Guid(silverStageMessage.OperationId)
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);

        if (result.IsSuccess && result.Value.Metadata.ContainsKey(EtlMetadataKeys.SilverLayerCompleted) && bool.Parse(result.Value.Metadata[EtlMetadataKeys.SilverLayerCompleted]))
        {
            await _goldenStageQueue.SendMessageAsync(JsonSerializer.Serialize(new GoldenStageMessage
            {
                OperationId = silverStageMessage.OperationId
            }), cancellationToken);
        }

        await _silverStageQueue.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
    }

    private bool TryToDeserializeMessage(string message, out SilverStageMessage silverStageMessage)
    {
        var success = false;
        silverStageMessage = null!;

        try
        {
            silverStageMessage = JsonSerializer.Deserialize<SilverStageMessage>(message)!;
            success = silverStageMessage != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message");
        }

        return success;
    }
}
