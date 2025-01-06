using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.PdfIngest.FunctionApp;

public class ExtractArticleParagraphsFunction
{
    private readonly ILogger<ExtractArticleParagraphsFunction> _logger;
    private readonly IArticleParagraphsExtractionChoreographyService _choreographyService;

    public ExtractArticleParagraphsFunction(ILogger<ExtractArticleParagraphsFunction> logger, IArticleParagraphsExtractionChoreographyService choreographyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(ExtractArticleParagraphsFunction))]
    public async Task RunAsync([QueueTrigger("extract-article-paragraphs-queue")] QueueMessage message, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Queue trigger function [extract-article-paragraphs-queue] started: {message.MessageText}");

        if(!Utils.TryToDeserializeMessage<ExtractArticleParagraphsMessage>(message.MessageText, out var extractArticleParagraphsMessage))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var palavenMessage = new Message<ExtractArticleParagraphsMessage>
        {
            Body = extractArticleParagraphsMessage,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        var result = await _choreographyService.ExtractArticleParagraphsAsync(palavenMessage, cancellationToken);

        if (result.HasErrors)
        {
            foreach (var error in result.ValidationErrors)
            {
                _logger.LogError(error.ErrorMessage);
            }
            foreach (var error in result.Exceptions)
            {
                _logger.LogError(error.Message);
            }

            return;
        }

        _logger.LogInformation($"C# Queue trigger function [extract-article-paragraphs-queue] processed: {message.MessageText}");                
    }    
}
