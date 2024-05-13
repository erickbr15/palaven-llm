using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IRagBertScoreMetricsDataService
{
    Task<RagBertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<RagBertScoreEvaluationMetric> CreateAsync(RagBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<RagBertScoreEvaluationMetric> GetAll();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<RagBertScoreEvaluationMetric> UpdateAsync(int id, RagBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
}
