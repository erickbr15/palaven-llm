using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.DatasetManagement;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.PdfConsumption.FunctionApp;

public class CreateInstructionsDatasetFunction
{
    private readonly ILogger<CreateInstructionsDatasetFunction> _logger;
    private readonly ICreateInstructionDatasetChoreographyService _choreographyService;

    public CreateInstructionsDatasetFunction(ILogger<CreateInstructionsDatasetFunction> logger, ICreateInstructionDatasetChoreographyService choreographyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(CreateInstructionsDatasetFunction))]
    public async Task RunAsync([QueueTrigger("addinstructions-todataset-queue")] QueueMessage message, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

        if (!Utils.TryToDeserializeMessage<CreateInstructionDatasetMessage>(message.MessageText, out var createInstructionsDatasetMessageBody))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var createInstructionsDatasetMessage = new Message<CreateInstructionDatasetMessage>
        {
            Body = createInstructionsDatasetMessageBody,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        var result = await _choreographyService.CreateInstructionDatasetAsync(createInstructionsDatasetMessage, cancellationToken);

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

        _logger.LogInformation($"C# Queue trigger function [addinstructions-todataset-queue] processed: {message.MessageText}");
    }
}
