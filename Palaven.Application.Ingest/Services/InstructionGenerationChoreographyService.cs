using Liara.Common;
using Liara.Common.Abstractions;
using Liara.Common.Abstractions.Cqrs;
using Liara.Common.Abstractions.Persistence;
using Palaven.Application.Abstractions.Ingest;
using Palaven.Application.Model.Ingest;
using Palaven.Infrastructure.Abstractions.Messaging;
using Palaven.Infrastructure.Model.Messaging;
using Palaven.Infrastructure.Model.Persistence.Documents;

namespace Palaven.Application.Ingest.Services;

public class InstructionGenerationChoreographyService : IInstructionGenerationChoreographyService
{
    private readonly IMessageQueueService _messageQueueService;
    private readonly ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> _generateInstructionsCommandHandler;
    private readonly IDocumentRepository<GoldenDocument> _goldenDocumentRepository;

    public InstructionGenerationChoreographyService(IMessageQueueService messageQueueService, ICommandHandler<GenerateInstructionsCommand, InstructionGenerationResult> commandHandler,
        IDocumentRepository<GoldenDocument> goldenDocumentRepository)
    {
        _messageQueueService = messageQueueService ?? throw new ArgumentNullException(nameof(messageQueueService));
        _generateInstructionsCommandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _goldenDocumentRepository = goldenDocumentRepository ?? throw new ArgumentNullException(nameof(goldenDocumentRepository));
    }

    public async Task<IResult> EnqueueInstructionTransformationTasksAsync(EnqueueInstructionTransformationTasksCommand command, CancellationToken cancellationToken)
    {
        var queryText = $"SELECT * FROM c WHERE c.tenant_id='{command.TenantId}' and c.trace_id='{command.OperationId}'";
        var documents = await _goldenDocumentRepository.GetAsync(queryText, continuationToken: null, cancellationToken: cancellationToken);

        if (!documents.Any())
        {
            return Result.Success();
        }
        
        var batchDocumentIds = new List<Guid>();
        var documentsArray = documents.ToArray();

        for (int i = 0; i < documentsArray.Length; i++)
        {
            if(batchDocumentIds.Count < command.BatchSize)
            {
                batchDocumentIds.Add(documentsArray[i].Id);
            }
            else
            {
                var createInstructionDatasetMessage = new CreateInstructionDatasetMessage
                {
                    OperationId = command.OperationId.ToString(),
                    TenantId = command.TenantId.ToString(),
                    DocumentIds = batchDocumentIds.Select(d => d.ToString()).ToArray()
                };

                await _messageQueueService.SendMessageAsync(createInstructionDatasetMessage, cancellationToken);


                var indexInstructionMessage = new IndexInstructionsMessage
                {
                    OperationId = command.OperationId.ToString(),
                    TenantId = command.TenantId.ToString(),
                    DocumentIds = batchDocumentIds.Select(d => d.ToString()).ToArray()
                };

                await _messageQueueService.SendMessageAsync(indexInstructionMessage, cancellationToken);

                batchDocumentIds.Clear();
            }
        }

        if (batchDocumentIds.Any())
        {
            var createInstructionDatasetMessage = new CreateInstructionDatasetMessage
            {
                OperationId = command.OperationId.ToString(),
                TenantId = command.TenantId.ToString(),
                DocumentIds = batchDocumentIds.Select(d => d.ToString()).ToArray()
            };

            await _messageQueueService.SendMessageAsync(createInstructionDatasetMessage, cancellationToken);

            var indexInstructionMessage = new IndexInstructionsMessage
            {
                OperationId = command.OperationId.ToString(),
                TenantId = command.TenantId.ToString(),
                DocumentIds = batchDocumentIds.Select(d => d.ToString()).ToArray()
            };

            await _messageQueueService.SendMessageAsync(indexInstructionMessage, cancellationToken);

            batchDocumentIds.Clear();
        }

        return Result.Success();
    }

    public async Task<IResult<InstructionGenerationResult>> GenerateInstructionsAsync(Message<GenerateInstructionsMessage> message, CancellationToken cancellationToken)
    {
        var command = new GenerateInstructionsCommand
        {
            OperationId = new Guid(message.Body.OperationId),
            DocumentIds = message.Body.DocumentIds.Select(d=> new Guid(d)).ToArray()
        };

        var result = await _generateInstructionsCommandHandler.ExecuteAsync(command, cancellationToken);
        if (result.HasErrors)
        {
            return result;
        }

        var indexMessage = new IndexInstructionsMessage
        {
            OperationId = message.Body.OperationId,
            TenantId = message.Body.TenantId,
            DocumentIds = result.Value.SuccessfulDocumentIds.Select(d => d.ToString()).ToArray()
        };
        await _messageQueueService.SendMessageAsync(indexMessage, cancellationToken);

        var instructionDatasetMessage = new CreateInstructionDatasetMessage
        {
            OperationId = message.Body.OperationId,
            TenantId = message.Body.TenantId,
            DocumentIds = result.Value.SuccessfulDocumentIds.Select(d => d.ToString()).ToArray()
        };

        await _messageQueueService.SendMessageAsync(instructionDatasetMessage, cancellationToken);

        await _messageQueueService.DeleteMessageAsync(message, cancellationToken);

        return result;
    }
}