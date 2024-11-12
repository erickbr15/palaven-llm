using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Persistence.Abstractions;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Services;

public class ArticleParagraphsExtractionChoreographyService : IArticleParagraphsExtractionChoreographyService
{
    private readonly ICommandHandler<ExtractArticleParagraphsCommand, EtlTaskDocument> _commandHandler;
    private readonly IDocumentRepository<SilverDocument> _silverStageRepository;
    private readonly IMessageQueueService _messageQueueService;

    public ArticleParagraphsExtractionChoreographyService(ICommandHandler<ExtractArticleParagraphsCommand, EtlTaskDocument> commandHandler, 
        IDocumentRepository<SilverDocument> silverStageRepository,
        IMessageQueueService messageQueueService)
    {
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _silverStageRepository = silverStageRepository ?? throw new ArgumentNullException(nameof(silverStageRepository));
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
    }

    public async Task<IResult<EtlTaskDocument>> ExtractArticleParagraphsAsync(Message<ExtractArticleParagraphsMessage> message, CancellationToken cancellationToken)
    {
        var command = new ExtractArticleParagraphsCommand
        {
            OperationId = new Guid(message.Body.OperationId)
        };

        var result = await _commandHandler.ExecuteAsync(command, cancellationToken);
        if(result.HasErrors)
        {
            return result;
        }

        var curateArticleMessages = await CreateCurateArticleMessagesAsync(result.Value, cancellationToken);
        
        await EnqueueCurateArticleMessagesAsync(curateArticleMessages, cancellationToken);

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return result;
    }

    private async Task<IList<CurateArticlesMessage>> CreateCurateArticleMessagesAsync(EtlTaskDocument etlTask, CancellationToken cancellationToken)
    {
        var queryText = $"SELECT * FROM c WHERE c.trace_id = '{etlTask.Id}'";

        var silverDocuments = await _silverStageRepository.GetAsync(queryText, continuationToken: null, cancellationToken);

        var processingQueue = new Queue<SilverDocument>(silverDocuments);

        var silverDocumentIdBatch = new List<string>();

        const int BATCH_SIZE = 10;

        var curateArticleMessages = new List<CurateArticlesMessage>();

        while (processingQueue.Count > 0)
        {
            silverDocumentIdBatch.Clear();

            while (silverDocumentIdBatch.Count < BATCH_SIZE && processingQueue.Count > 0)
            {
                var silverDocument = processingQueue.Dequeue();
                silverDocumentIdBatch.Add(silverDocument.Id.ToString());
            }

            var curateArticleMessage = new CurateArticlesMessage
            {
                OperationId = etlTask.Id,
                DocumentIds = silverDocumentIdBatch.ToArray()
            };

            curateArticleMessages.Add(curateArticleMessage);
        }

        return curateArticleMessages;
    }

    private async Task EnqueueCurateArticleMessagesAsync(IList<CurateArticlesMessage> curateArticleMessages, CancellationToken cancellationToken)
    {
        foreach (var curateArticleMessage in curateArticleMessages)
        {
            await _messageQueueService.SendMessageAsync(curateArticleMessage, cancellationToken);
        }
    }
}