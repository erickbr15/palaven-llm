﻿using Palaven.Infrastructure.Model.Persistence.Entities;
using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Infrastructure.Abstractions.Persistence;

public interface IEvaluationSessionDataService
{
    Task<EvaluationSession?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    Task<EvaluationSession> CreateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);
    Task AddInstructionToEvaluationSessionAsync(IEnumerable<EvaluationSessionInstruction> instructions, CancellationToken cancellationToken);
    Task<EvaluationSession> UpdateEvaluationSessionAsync(EvaluationSession evaluationSession, CancellationToken cancellationToken);
    Task UpsertChatCompletionResponseAsync(IEnumerable<LlmResponse> chatCompletionResponses, CancellationToken cancellationToken);
    void CleanChatCompletionResponses(Func<LlmResponse, bool> selectionCriteria, Func<string?, string> cleaningStrategy);
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Func<LlmResponseView, bool> selectionCriteria);
    IList<InstructionEntity> FetchChatCompletionLlmInstructions(Guid evaluationSessionId, int evaluationExerciseId, int batchNumber);
    IQueryable<EvaluationSession> GetEvaluationSessionQuery(Func<EvaluationSession, bool> criteria);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

}
