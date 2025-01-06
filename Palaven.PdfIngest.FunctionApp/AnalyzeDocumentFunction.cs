using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.PdfIngest.FunctionApp;

public class AnalyzeDocumentFunction
{
    private readonly ILogger<AnalyzeDocumentFunction> _logger;    
    private readonly IDocumentAnalysisChoreographyService _choreographyService;

    public AnalyzeDocumentFunction(ILogger<AnalyzeDocumentFunction> logger, IDocumentAnalysisChoreographyService choreographyService)
    {                
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(AnalyzeDocumentFunction))]
    public async Task RunAsync([QueueTrigger("analyze-documents-queue")] QueueMessage message, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Queue trigger function [analyze-documents-queue] triggered. Message: {message.MessageText}");

        if (!Utils.TryToDeserializeMessage<DocumentAnalysisMessage>(message.MessageText, out var documentAnalysisMessage))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var palavenMessage = new Message<DocumentAnalysisMessage>
        {
            Body = documentAnalysisMessage,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        await _choreographyService.StartDocumentAnalysisAsync(palavenMessage, cancellationToken);

        _logger.LogInformation($"Queue trigger function [analyze-documents-queue] processed: {message.MessageText}");
    }
}
