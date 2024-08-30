using Palaven.Model.Entities;
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
    void CleanChatCompletionResponses(Func<FineTunedLlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);    
    void CleanChatCompletionResponses(Func<FineTunedLlmWithRagResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);    
    void CleanChatCompletionResponses(Func<LlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);    
    void CleanChatCompletionResponses(Func<LlmWithRagResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);
    Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken);
    Task UpsertChatCompletionPerformanceEvaluationAsync(IEnumerable<RougeScoreMetric> rougeScoreMetrics, CancellationToken cancellationToken);
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Func<LlmResponseView, bool> selectionCriteria);
    IList<LlmResponseView> FetchChatCompletionLlmWithRagResponses(Func<LlmResponseView, bool> selectionCriteria);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
}
