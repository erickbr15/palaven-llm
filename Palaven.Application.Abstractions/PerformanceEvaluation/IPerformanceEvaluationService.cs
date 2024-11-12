using Liara.Common.Abstractions;
using Palaven.Application.Model.DatasetManagement;
using Palaven.Application.Model.PerformanceEvaluation;
using Palaven.Infrastructure.Model.Persistence.Views;

namespace Palaven.Application.PerformanceEvaluation;

public interface IPerformanceEvaluationService
{
    Task<EvaluationSessionInfo?> GetEvaluationSessionAsync(Guid sessionId, CancellationToken cancellationToken);
    EvaluationSessionInfo? GetActiveEvaluationSessionByDataset(Guid datasetId);
    Task<IResult<EvaluationSessionInfo>> CreateEvaluationSessionAsync(CreateEvaluationSessionCommand command, CancellationToken cancellationToken);
    Task<IResult> UpsertChatCompletionResponseAsync(UpsertChatCompletionResponseCommand command, CancellationToken cancellationToken);
    Task<IResult> CleanChatCompletionResponseAsync(CleanChatCompletionResponseCommand command, CancellationToken cancellationToken);
    IList<LlmResponseView> FetchChatCompletionLlmResponses(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType);
    IList<InstructionData> FetchChatCompletionLlmInstructions(Guid evaluationSessionId, int batchNumber, string chatCompletionExcerciseType);    
}
