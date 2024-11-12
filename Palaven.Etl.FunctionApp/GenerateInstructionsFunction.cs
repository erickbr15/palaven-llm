using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Etl.FunctionApp;

public class GenerateInstructionsFunction
{
    private readonly ILogger<GenerateInstructionsFunction> _logger;
    private readonly IInstructionGenerationChoreographyService _choreographyService;

    public GenerateInstructionsFunction(ILogger<GenerateInstructionsFunction> logger, IInstructionGenerationChoreographyService choreographyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(GenerateInstructionsFunction))]
    public async Task Run([QueueTrigger("generate-instructions-queue", Connection = "AzureStorageLawDocs")] QueueMessage message, FunctionContext context)
    {
        _logger.LogInformation($"C# Queue trigger function [generate-instructions-queue] started: {message.MessageText}");

        if (!Utils.TryToDeserializeMessage<GenerateInstructionsMessage>(message.MessageText, out var generateInstructionMessageBody))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var generateInstructionsMessage = new Message<GenerateInstructionsMessage>
        {
            Body = generateInstructionMessageBody,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        var result = await _choreographyService.GenerateInstructionsAsync(generateInstructionsMessage, context.CancellationToken);

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

        _logger.LogInformation($"C# Queue trigger function [generate-instructions-queue] processed: {message.MessageText}");
    }
}
