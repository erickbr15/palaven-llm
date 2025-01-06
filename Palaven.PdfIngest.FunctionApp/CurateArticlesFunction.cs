using Azure.Storage.Queues.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.PdfIngest.FunctionApp;

public class CurateArticlesFunction
{
    private readonly ILogger<CurateArticlesFunction> _logger;
    private readonly IArticlesCurationChoreographyService _choreographyService;

    public CurateArticlesFunction(ILogger<CurateArticlesFunction> logger, IArticlesCurationChoreographyService choreographyService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(CurateArticlesFunction))]
    public async Task RunAsync([QueueTrigger("curate-articles-queue")] QueueMessage message, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Queue trigger function [curate-articles-queue] started: {message.MessageText}");

        if(!Utils.TryToDeserializeMessage<CurateArticlesMessage>(message.MessageText, out var curateArticleMessageBody))
        {
            _logger.LogError($"Failed to deserialize message. Message: {message.MessageText}");
            return;
        }

        var curateArticleMessage = new Message<CurateArticlesMessage>
        {
            Body = curateArticleMessageBody,
            DequeueCount = message.DequeueCount,
            ExpiresOn = message.ExpiresOn,
            InsertedOn = message.InsertedOn,
            MessageId = message.MessageId,
            NextVisibleOn = message.NextVisibleOn,
            PopReceipt = message.PopReceipt
        };

        var result = await _choreographyService.CurateArticlesAsync(curateArticleMessage, cancellationToken);

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

        _logger.LogInformation($"Queue trigger function [curate-articles-queue] processed: {message.MessageText}");
    }
}
