using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Abstractions.Storage;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Ingest.Services;

public class DocumentAnalysisChoreographyService : IDocumentAnalysisChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly IEtlInboxStorage _etlInboxStorage;
    private readonly ICommandHandler<AnalyzeDocumentCommand, string> _startDocumentAnalysisCommandHanlder;

    public DocumentAnalysisChoreographyService(IMessageQueueService messageQueueService, IEtlInboxStorage etlInboxStorage, 
        ICommandHandler<AnalyzeDocumentCommand, string> startDocumentAnalysisCommandHandler)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _etlInboxStorage = etlInboxStorage ?? throw new ArgumentNullException(nameof(etlInboxStorage));
        _startDocumentAnalysisCommandHanlder = startDocumentAnalysisCommandHandler ?? throw new ArgumentNullException(nameof(startDocumentAnalysisCommandHandler)); 
    }

    public async Task<IResult<string>> StartDocumentAnalysisAsync(Message<DocumentAnalysisMessage> message, CancellationToken cancellationToken)
    {
        var blob = await _etlInboxStorage.DownloadAsync(message.Body.DocumentBlobName, cancellationToken);

        var command = new AnalyzeDocumentCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentLocale = message.Body.Locale,
            DocumentContent = blob.BinaryData.ToStream()
        };

        command.DocumentPages.ToList().AddRange(message.Body.Pages);

        var result = await _startDocumentAnalysisCommandHanlder.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            return result;
        }

        var bronzeStageMessage = new ExtractDocumentPagesMessage
        {
            OperationId = message.Body.OperationId,
            DocumentAnalysisOperationId = result.Value
        };

        await _messageQueueService.SendMessageAsync(bronzeStageMessage, cancellationToken);
        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return result;
    }
}
