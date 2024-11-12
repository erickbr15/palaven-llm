using Liara.Common.Abstractions;
using Palaven.Application.Model.PerformanceMetrics;

namespace Palaven.Application.Abstractions.PerformanceMetrics;

public interface IPerformanceMetricsService
{
    Task<IResult> UpsertBertscoreBatchEvaluationAsync(UpsertBertScoreBatchEvaluationRequest request, CancellationToken cancellationToken);
    IList<BertScoreMetrics> FetchEvaluationSessionBertscoreMetrics(Guid evaluationSessionId, string evaluationExercise);
    Task<IResult> UpsertRougeScoreBatchEvaluationAsync(IEnumerable<UpsertRougeScoreBatchEvaluationRequest> requests, CancellationToken cancellationToken);
    IList<RougeScoreMetrics> FetchEvaluationSessionRougeScoreMetrics(Guid evaluationSessionId, string evaluationExercise, string rougeType);
    Task<IResult> UpsertBleuBatchEvaluationAsync(UpsertBleuBatchEvaluationRequest request, CancellationToken cancellationToken);
    IList<BleuMetrics> FetchEvaluationSessionBleuMetrics(Guid evaluationSessionId, string evaluationExercise);
}
