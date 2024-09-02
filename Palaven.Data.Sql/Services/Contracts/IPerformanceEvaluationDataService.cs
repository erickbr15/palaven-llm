using Palaven.Model.Entities;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IPerformanceEvaluationDataService
{
    Task<EvaluationSession?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<EvaluationSession> CreateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);
    Task AddInstructionToEvaluationSessionAsync(IEnumerable<EvaluationSessionInstruction> instructions, CancellationToken cancellationToken);
    Task<EvaluationSession> UpdateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);    
    Task UpsertChatCompletionResponseAsync(IEnumerable<LlmResponse> chatCompletionResponses, CancellationToken cancellationToken);    
    void CleanChatCompletionResponses(Func<LlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);          
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Func<LlmResponseView, bool> selectionCriteria);
    IQueryable<EvaluationSession> GetEvaluationSessionQuery(Func<EvaluationSession, bool> criteria);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    
}
