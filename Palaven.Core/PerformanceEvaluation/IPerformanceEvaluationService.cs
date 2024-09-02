using Liara.Common;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Core.PerformanceEvaluation;

public interface IPerformanceEvaluationService
{
    Task<EvaluationSessionInfo?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    EvaluationSessionInfo? GetActiveEvaluationSessionByDataset(Guid datasetId);
    Task<IResult<EvaluationSessionInfo?>> CreateEvaluationSessionAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken);         
    Task<IResult> UpsertChatCompletionResponseAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken);    
    Task<IResult> CleanChatCompletionResponseAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken);
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType);
    IList<InstructionData> FetchChatCompletionLlmInstructions(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType);
    Task<IResult> UpsertBertscoreBatchEvaluationAsync(UpsertBertscoreBatchEvaluationCommand command, CancellationToken cancellationToken);
    IList<EvaluationSessionBertscoreMetrics> FetchEvaluationSessionBertscoreMetrics(Guid evaluationSessionId, string evaluationExercise);
    Task<IResult> UpsertRougeScoreBatchEvaluationAsync(IEnumerable<UpsertRougescoreBatchEvaluationCommand> commands, CancellationToken cancellationToken);
    IList<EvaluationSessionRougescoreMetrics> FetchEvaluationSessionRougeScoreMetrics(Guid evaluationSessionId, string evaluationExercise);
}
