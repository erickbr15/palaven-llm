using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IPerformanceEvaluationDataService
{
    Task<EvaluationSession?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<EvaluationSession> CreateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);
    Task<EvaluationSession> UpdateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);    
    Task UpsertChatCompletionResponseAsync(IEnumerable<FineTunedLlmResponse> chatCompletionResponses, CancellationToken cancellationToken);
    Task UpsertChatCompletionResponseAsync(IEnumerable<FineTunedLlmWithRagResponse> chatCompletionResponses, CancellationToken cancellationToken);
    Task UpsertChatCompletionResponseAsync(IEnumerable<LlmResponse> chatCompletionResponses, CancellationToken cancellationToken);
    Task UpsertChatCompletionResponseAsync(IEnumerable<LlmWithRagResponse> chatCompletionResponses, CancellationToken cancellationToken);
    Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric chatCompletionPerformanceEvaluation, CancellationToken cancellationToken);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
}
