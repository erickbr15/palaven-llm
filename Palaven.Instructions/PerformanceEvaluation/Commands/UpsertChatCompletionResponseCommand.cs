using Liara.Common;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.PerformanceEvaluation;
using Palaven.Model.PerformanceEvaluation.Commands;

namespace Palaven.Core.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseCommand : ICommand<IEnumerable<UpsertChatCompletionResponseModel>, bool>
{
    private readonly IPerformanceEvaluationDataService _performanceEvaluationDataService;

    public UpsertChatCompletionResponseCommand(IPerformanceEvaluationDataService performanceEvaluationDataService)
    {
        _performanceEvaluationDataService = performanceEvaluationDataService ?? throw new ArgumentNullException(nameof(performanceEvaluationDataService));
    }

    public async Task<IResult<bool>> ExecuteAsync(IEnumerable<UpsertChatCompletionResponseModel> inputModel, CancellationToken cancellationToken)
    {
        await UpsertVanillaResponsesAsync(inputModel, cancellationToken);
        await UpsertRagResponsesAsync(inputModel, cancellationToken);
        await UpsertFineTunedResponsesAsync(inputModel, cancellationToken);
        await UpsertFineTunedAndRagResponsesAsync(inputModel, cancellationToken);

        await _performanceEvaluationDataService.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    private Task UpsertVanillaResponsesAsync(IEnumerable<UpsertChatCompletionResponseModel> responses, CancellationToken cancellationToken)
    {
        var llmVanillaResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmVanilla, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmVanillaResponses, cancellationToken);
    }

    private Task UpsertRagResponsesAsync(IEnumerable<UpsertChatCompletionResponseModel> responses, CancellationToken cancellationToken)
    {
        var llmRagResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmWithRag, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmRagResponses, cancellationToken);
    }

    private Task UpsertFineTunedResponsesAsync(IEnumerable<UpsertChatCompletionResponseModel> responses, CancellationToken cancellationToken)
    {
        var llmFineTunedResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmFineTuned, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmFineTunedResponses, cancellationToken);
    }

    private Task UpsertFineTunedAndRagResponsesAsync(IEnumerable<UpsertChatCompletionResponseModel> responses, CancellationToken cancellationToken)
    {
        var llmFineTunedAndRagResponses = responses
            .Where(r => string.Equals(r.ChatCompletionExcerciseType, ChatCompletionExcerciseType.LlmFineTunedAndRag, StringComparison.OrdinalIgnoreCase))
            .Select(r => new LlmResponse
            {
                BatchNumber = r.BatchNumber,
                InstructionId = r.InstructionId,
                ResponseCompletion = r.ResponseCompletion,
                SessionId = r.SessionId!.Value
            }).ToList();
        

        return _performanceEvaluationDataService.UpsertChatCompletionResponseAsync(llmFineTunedAndRagResponses, cancellationToken);
    }
}
