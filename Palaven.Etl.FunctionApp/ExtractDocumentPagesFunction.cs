using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Etl.FunctionApp;

public class ExtractDocumentPagesFunction
{
    private readonly ILogger<ExtractDocumentPagesFunction> _logger;
    private readonly IMessageQueueService _messageQueueService;
    private readonly IDocumentPagesExtractionChoreographyService _choreographyService;

    public ExtractDocumentPagesFunction(ILoggerFactory loggerFactory, IMessageQueueService messageQueueService,
        IDocumentPagesExtractionChoreographyService choreographyService)
    {
        _logger = loggerFactory.CreateLogger<ExtractDocumentPagesFunction>();
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _choreographyService = choreographyService ?? throw new ArgumentNullException(nameof(choreographyService));
    }

    [Function(nameof(ExtractDocumentPagesFunction))]
    public async Task RunAsync([TimerTrigger("0 */3 * * * *")] TimerInfo myTimer, FunctionContext context, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        var message = await _messageQueueService.ReceiveMessageAsync<ExtractDocumentPagesMessage>(cancellationToken: cancellationToken);

        if (message is null)
        {
            _logger.LogInformation("No message to process.");
            return;
        }

        var result = await _choreographyService.ExtractPagesAsync(message, context.CancellationToken);

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

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }
    }
}
