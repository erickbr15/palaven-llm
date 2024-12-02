using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Abstractions.Notifications;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Services;

public class StartIngestionChoreographyService : IStartIngestionChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly INotificationService _notificationService;
    private readonly ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument> _startTaxLawIngestCommandHandler;        

    public StartIngestionChoreographyService(IMessageQueueService messageQueueService, INotificationService notificationService, 
        ICommandHandler<StartTaxLawIngestCommand, EtlTaskDocument> startTaxLawIngestCommandHandler)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _startTaxLawIngestCommandHandler = startTaxLawIngestCommandHandler ?? throw new ArgumentNullException(nameof(startTaxLawIngestCommandHandler));
    }    

    public async Task<IResult<EtlTaskDocument>> StartIngestionAsync(StartTaxLawIngestCommand command, CancellationToken cancellationToken)
    {
        if(command == null)
        {
            return Result<EtlTaskDocument>.Fail(new ArgumentNullException(nameof(command)));
        }

        var result = await _startTaxLawIngestCommandHandler.ExecuteAsync(command, cancellationToken);

        if (result.HasErrors)
        {
            return result;
        }

        var documentAnalysisMessage = result.Value.ToDocumentAnalysisMessage();

        await _messageQueueService.SendMessageAsync(documentAnalysisMessage, cancellationToken);

        await SendNotificationMessage(result.Value, cancellationToken);        

        return result;
    }

    private Task SendNotificationMessage(EtlTaskDocument etlTaskDocument, CancellationToken cancellationToken)
    {
        var message = string.Format(Resources.Etl.NotificationStartTaxLawIngest, etlTaskDocument.Id, etlTaskDocument.Metadata[EtlMetadataKeys.FileName]);
        return _notificationService.SendAsync(etlTaskDocument.TenantId, message, cancellationToken);
    }
}
