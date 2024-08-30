using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseCommandHandler : ICommandHandler<UpsertChatCompletionResponseCommand>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public UpsertChatCompletionResponseCommandHandler(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? 
            throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }        

    public async Task<IResult> ExecuteAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken)
    {
        await UpsertVanillaResponsesAsync(command.ChatCompletionResponses, cancellationToken);
        await UpsertRagResponsesAsync(command.ChatCompletionResponses, cancellationToken);
        await UpsertFineTunedResponsesAsync(command.ChatCompletionResponses, cancellationToken);
        await UpsertFineTunedAndRagResponsesAsync(command.ChatCompletionResponses, cancellationToken);

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private Task UpsertVanillaResponsesAsync(IEnumerable<ChatCompletionResponse> responses, CancellationToken cancellationToken)
    {
        var llmVanillaResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmVanilla, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmVanillaResponses, cancellationToken);
    }

    private Task UpsertRagResponsesAsync(IEnumerable<ChatCompletionResponse> responses, CancellationToken cancellationToken)
    {
        var llmRagResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmWithRag, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmWithRagResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmRagResponses, cancellationToken);
    }

    private Task UpsertFineTunedResponsesAsync(IEnumerable<ChatCompletionResponse> responses, CancellationToken cancellationToken)
    {
        var llmFineTunedResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmFineTuned, StringComparison.OrdinalIgnoreCase))
            .Select(r => new FineTunedLlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmFineTunedResponses, cancellationToken);
    }

    private Task UpsertFineTunedAndRagResponsesAsync(IEnumerable<ChatCompletionResponse> responses, CancellationToken cancellationToken)
    {
        var llmFineTunedAndRagResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmFineTunedAndRag, StringComparison.OrdinalIgnoreCase))
            .Select(r => new FineTunedLlmWithRagResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value,
                ElapsedTime = r.ElapsedTime!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmFineTunedAndRagResponses, cancellationToken);
    }    
}
