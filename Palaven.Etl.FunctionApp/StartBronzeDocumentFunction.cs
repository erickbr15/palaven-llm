using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Liara.Azure.Storage;
using Liara.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Palaven.Model;
using Palaven.Model.Ingest;

namespace Palaven.Etl.FunctionApp;

public class StartBronzeDocumentFunction
{
    private readonly ILogger<StartBronzeDocumentFunction> _logger;    
    private readonly BlobContainerClient _blobContainerClient;
    private readonly QueueClient _documentAnalysisClient;
    private readonly QueueClient _bronzeStageQueueClient;
    private readonly ICommandHandler<StartBronzeDocumentCommand, string> _commandHandler;

    public StartBronzeDocumentFunction(ILogger<StartBronzeDocumentFunction> logger, IOptions<AzureStorageOptions> storageOptionsService,
        IAzureClientFactory<BlobServiceClient> blobServiceClientFactory,
        IAzureClientFactory<QueueServiceClient> queueServiceClientFactory,
        ICommandHandler<StartBronzeDocumentCommand, string> commandHandler)
    {                
        var storageOptions = storageOptionsService.Value ?? throw new ArgumentNullException(nameof(storageOptionsService));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _blobContainerClient = blobServiceClientFactory.CreateClient("AzureStorageLawDocs")
            .GetBlobContainerClient(storageOptions.BlobContainers[BlobStorageContainers.EtlInbox]);

        _documentAnalysisClient = queueServiceClientFactory.CreateClient("AzureStorageLawDocs")
            .GetQueueClient(storageOptionsService.Value.QueueNames[QueueStorageNames.DocumentAnalysisQueue]);

        _bronzeStageQueueClient = queueServiceClientFactory.CreateClient("AzureStorageLawDocs")
            .GetQueueClient(storageOptionsService.Value.QueueNames[QueueStorageNames.BronzeStageQueue]);
        
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
    }

    [Function(nameof(StartBronzeDocumentFunction))]
    public async Task Run([QueueTrigger("document-analysis-queue", Connection = "AzureStorageLawDocs")] QueueMessage message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

        if (!TryToDeserializeMessage(message.MessageText, out var documentAnalysisMessage))
        {
            _logger.LogError("Failed to deserialize message");
            return;
        }

        var blobClient = _blobContainerClient.GetBlobClient(documentAnalysisMessage.DocumentBlobName);
        var blobDownloadResult = await blobClient.DownloadContentAsync(cancellationToken);
        var blobContent = blobDownloadResult.Value.Content.ToStream();

        var command = CreateStartBronzeDocumentCommand(documentAnalysisMessage, blobContent);
        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);

        if (result.IsSuccess)
        {
            await _bronzeStageQueueClient.SendMessageAsync(result.Value, cancellationToken: cancellationToken);
            await _documentAnalysisClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
        }
    }

    private bool TryToDeserializeMessage(string message, out DocumentAnalysisMessage documentAnalysisMessage)
    {
        var success = false;
        documentAnalysisMessage = null!;

        try
        {
            documentAnalysisMessage = JsonSerializer.Deserialize<DocumentAnalysisMessage>(message)!;
            success = documentAnalysisMessage != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize message");
        }

        return success;
    }    

    private StartBronzeDocumentCommand CreateStartBronzeDocumentCommand(DocumentAnalysisMessage documentAnalysisMessage, Stream blobContent)
    {
        var documentContent = new MemoryStream();
        blobContent.CopyTo(documentContent);

        return new StartBronzeDocumentCommand
        {
            OperationId = new Guid(documentAnalysisMessage.OperationId),
            DocumentLocale = documentAnalysisMessage.Locale,
            DocumentPages = documentAnalysisMessage.Pages,
            DocumentContent = documentContent
        };
    }
}
