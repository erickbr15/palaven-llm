using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;

namespace Palaven.Application.Ingest.Services;

public class ArticlesCurationChoreographyService : IArticlesCurationChoreographyService
{
    private readonly ICommandHandler<CurateArticlesCommand, ArticlesCurationResult> _curateArticlesCommandHandler;
    private readonly IMessageQueueService _messageQueueService;

    public ArticlesCurationChoreographyService(ICommandHandler<CurateArticlesCommand, ArticlesCurationResult> curateArticlesCommandHandler, IMessageQueueService messageQueueService)
    {
        _curateArticlesCommandHandler = curateArticlesCommandHandler ?? throw new ArgumentNullException(nameof(curateArticlesCommandHandler));
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
    }    

    public async Task<IResult> CurateArticlesAsync(Message<CurateArticlesMessage> message, CancellationToken cancellationToken)
    {                
        var command = new CurateArticlesCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(s=>new Guid(s)).ToArray()
        };

        var result = await _curateArticlesCommandHandler.ExecuteAsync(command, cancellationToken);
        if(result.HasErrors)
        {
            return result;
        }

        var generateInstructionsMessage = new GenerateInstructionsMessage
        {
            OperationId = result.Value.OperationId.ToString(),
            DocumentIds = result.Value.CuratedDocumentIds.Select(s => s.ToString()).ToArray()
        };

        await _messageQueueService.SendMessageAsync(generateInstructionsMessage, cancellationToken);

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return Result.Success();
    }
}
