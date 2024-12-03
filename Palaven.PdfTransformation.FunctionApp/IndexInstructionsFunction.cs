using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.VectorIndexing.Services;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.PdfTransformation.FunctionApp;

public class IndexInstructionsFunction
{
    private readonly ILogger<IndexInstructionsFunction> _logger;
    private readonly InstructionsIndexingChoreographyService _choreographyService;

    public IndexInstructionsFunction(ILogger<IndexInstructionsFunction> logger, InstructionsIndexingChoreographyService choreographyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(IndexInstructionsFunction))]
    public async Task RunAsync([QueueTrigger("index-instructions-queue")] QueueMessage message, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Queue trigger function [index-instructions-queue] started: {message.MessageText}");

        if(Utils.TryToDeserializeMessage<IndexInstructionsMessage>(message.MessageText, out var indexInstructionsMessageBody))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var indexInstructionsMessage = new Message<IndexInstructionsMessage>
        {
            Body = indexInstructionsMessageBody,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        var result = await _choreographyService.IndexInstructionsAsync(indexInstructionsMessage, cancellationToken);

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

        _logger.LogInformation($"C# Queue trigger function [index-instructions-queue] processed: {message.MessageText}");
    }
}
