using System.Text.Json;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Liara.Azure.Storage;
using Liara.Clients.OpenAI.Model.Chat;
using Liara.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palaven.Model;
using Palaven.Model.Data.Documents;
using Palaven.Model.Ingest;

namespace Palaven.Etl.FunctionApp;

public class CompleteBronzeDocumentFunction
{
    private readonly ILogger<CompleteBronzeDocumentFunction> _logger;
    private readonly QueueClient _bronzeStageQueue;    
    private readonly QueueClient _silverStageQueue;
    private readonly ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument> _commandHandler;    

    public CompleteBronzeDocumentFunction(ILoggerFactory loggerFactory, IOptions<AzureStorageOptions> storageOptionService,
        IAzureClientFactory<QueueServiceClient> azureClientFactory,
        ICommandHandler<CompleteBronzeDocumentCommand, EtlTaskDocument> commandHandler)
    {
        _logger = loggerFactory.CreateLogger<CompleteBronzeDocumentFunction>();

        var storageOptions = storageOptionService?.Value ?? throw new ArgumentNullException(nameof(storageOptionService));

        var queueServiceClient = azureClientFactory.CreateClient("AzureStorageLawDocs") ??
            throw new InvalidOperationException($"Unable to create the queue client for AzureStorageLawDocs");

        _bronzeStageQueue = queueServiceClient.GetQueueClient(storageOptions.QueueNames[QueueStorageNames.DocumentAnalysisQueue]);
        _silverStageQueue = queueServiceClient.GetQueueClient(storageOptions.QueueNames[QueueStorageNames.SilverStageQueue]);

        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    [Function(nameof(CompleteBronzeDocumentFunction))]
    public async Task Run([TimerTrigger("0 */3 * * * *")] TimerInfo timer, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        if(timer.ScheduleStatus is null)
        {
            return;
        }

        var azureResponse = await _bronzeStageQueue.ReceiveMessageAsync(cancellationToken: cancellationToken);
        if (!TryToDeserializeMessage(azureResponse.Value.Body.ToString(), out var analysisMessage))
        {
            await _bronzeStageQueue.DeleteMessageAsync(azureResponse.Value.MessageId, azureResponse.Value.PopReceipt, cancellationToken);
            return;
        }

        var command = new CompleteBronzeDocumentCommand 
        {
            OperationId = new Guid(analysisMessage.OperationId),
            DocumentAnalysisOperationId = analysisMessage.DocumentAnalysisOperationId
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);

        await EnqueueSilverStageMessageAsync(result, cancellationToken);
        await UpdateBronzeStageQueueStateAsync(azureResponse.Value, result, cancellationToken);
    }

    private bool TryToDeserializeMessage(string message, out BronzeStageMessage bronzeStageMessage)
    {
        var success = false;
        bronzeStageMessage = null!;

        try
        {
            bronzeStageMessage = JsonSerializer.Deserialize<BronzeStageMessage>(message)!;
            success = bronzeStageMessage != null;            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message");
        }

        return success;
    }

    private async Task UpdateBronzeStageQueueStateAsync(QueueMessage bronzeStageMessage, IResult<EtlTaskDocument> bronzeStageProcessResult, CancellationToken cancellationToken)
    {
        if(bronzeStageProcessResult.IsSuccess && 
            !bronzeStageProcessResult.Value.Metadata.ContainsKey(EtlMetadataKeys.BronzeLayerProcessed))
        {            
            return;
        }
        await _bronzeStageQueue.DeleteMessageAsync(bronzeStageMessage.MessageId, bronzeStageMessage.PopReceipt, cancellationToken);
    }    
    
    private async Task EnqueueSilverStageMessageAsync(IResult<EtlTaskDocument> bronzeStageProcessResult, CancellationToken cancellationToken)
    {
        if (bronzeStageProcessResult.IsSuccess && bool.Parse(bronzeStageProcessResult.Value.Metadata[EtlMetadataKeys.BronzeLayerCompleted]))
        {
            var silverStageMessage = new SilverStageMessage
            {
                OperationId = bronzeStageProcessResult.Value.Id
            };

            var message = JsonSerializer.Serialize(silverStageMessage);
            await _silverStageQueue.SendMessageAsync(message, cancellationToken: cancellationToken);            
        }
    }
}
