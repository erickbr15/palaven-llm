using Liara.Common;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation;

public interface IPerformanceEvaluationService
{
    Task<EvaluationSessionInfo?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<IResult<EvaluationSessionInfo?>> CreateEvaluationSessionAsync(CreateEvaluationSessionModel model, CancellationToken cancellationToken);         
    Task<IResult> UpsertChatCompletionResponseAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken);
    Task<IResult> UpsertChatCompletionPerformanceEvaluationAsync(UpsertChatCompletionPerformanceEvaluation model, CancellationToken cancellationToken);
    Task<IResult> CleanChatCompletionResponseAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken);
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType);
}
