using Liara.Common.Abstractions;
using Liara.Common;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;
using Palaven.Infrastructure.Abstractions.Messaging;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Infrastructure.Model.AI;
using Palaven.Application.Abstractions.Ingest;

namespace Palaven.Application.Ingest.Services;

public class DocumentPagesExtractionChoreographyService : IDocumentPagesExtractionChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<GetDocumentAnalysisResultCommand, DocumentAnalysisResult> _getDocumentAnalysisResultCommandHandler;
    private readonly ICommandHandler<ExtractDocumentPagesCommand, EtlTaskDocument> _createBronzeLayerCommandHandler;

    public DocumentPagesExtractionChoreographyService(IMessageQueueService messageQueueService, ICommandHandler<GetDocumentAnalysisResultCommand, DocumentAnalysisResult> getDocumentAnalysisResultCommandHandler,
        ICommandHandler<ExtractDocumentPagesCommand, EtlTaskDocument> createBronzeLayerCommandHandler)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _getDocumentAnalysisResultCommandHandler = getDocumentAnalysisResultCommandHandler ?? throw new ArgumentNullException(nameof(getDocumentAnalysisResultCommandHandler));
        _createBronzeLayerCommandHandler = createBronzeLayerCommandHandler ?? throw new ArgumentNullException(nameof(createBronzeLayerCommandHandler));
    }

    public async Task<IResult<EtlTaskDocument>> ExtractPagesAsync(Message<ExtractDocumentPagesMessage> message, CancellationToken cancellationToken)
    {
        var documentAnalysisResult = await GetDocumentAnalysisResultAsync(message.Body, cancellationToken);

        if (documentAnalysisResult.HasErrors)
        {
            var result = Result<EtlTaskDocument>.Fail(documentAnalysisResult.ValidationErrors, documentAnalysisResult.Exceptions);
            result.Value = documentAnalysisResult.Value.EtlRelatedTask;

            return result;
        }

        if (documentAnalysisResult.IsSuccess && !documentAnalysisResult.Value.IsComplete)
        {
            return Result<EtlTaskDocument>.Success(documentAnalysisResult.Value.EtlRelatedTask);
        }

        var bronzeLayerCreationResult = await CreateBronzeLayerAsync(message.Body, documentAnalysisResult.Value, cancellationToken);

        if (bronzeLayerCreationResult.HasErrors)
        {
            return bronzeLayerCreationResult;
        }

        if(bronzeLayerCreationResult.IsSuccess && (!bronzeLayerCreationResult.Value.Metadata.ContainsKey(EtlMetadataKeys.BronzeLayerCompleted) ||
            !bool.Parse(bronzeLayerCreationResult.Value.Metadata[EtlMetadataKeys.BronzeLayerCompleted])))
        {
            return bronzeLayerCreationResult;
        }        

        await _messageQueueService.SendMessageAsync(new ExtractArticleParagraphsMessage 
        { 
            OperationId = bronzeLayerCreationResult.Value.Id.ToString(),
            TenantId = message.Body.TenantId
        }, cancellationToken);

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return bronzeLayerCreationResult;
    }

    private async Task<IResult<DocumentAnalysisResult>> GetDocumentAnalysisResultAsync(ExtractDocumentPagesMessage message, CancellationToken cancellationToken)
    {
        var documentAnalysisResultCommand = new GetDocumentAnalysisResultCommand
        {
            OperationId = new Guid(message.OperationId),
            DocumentAnalysisOperationId = message.DocumentAnalysisOperationId
        };

        var documentAnalysisResult = await _getDocumentAnalysisResultCommandHandler.ExecuteAsync(documentAnalysisResultCommand, cancellationToken);

        return documentAnalysisResult;
    }

    private async Task<IResult<EtlTaskDocument>> CreateBronzeLayerAsync(ExtractDocumentPagesMessage message, DocumentAnalysisResult documentAnalysisResult, CancellationToken cancellationToken)
    {
        var createBronzeLayerCommand = new ExtractDocumentPagesCommand
        {
            OperationId = new Guid(message.OperationId),
            Paragraphs = documentAnalysisResult.Paragraphs
        };

        var bronzeLayerCreationResult = await _createBronzeLayerCommandHandler.ExecuteAsync(createBronzeLayerCommand, cancellationToken);

        return bronzeLayerCreationResult;
    }    
}
