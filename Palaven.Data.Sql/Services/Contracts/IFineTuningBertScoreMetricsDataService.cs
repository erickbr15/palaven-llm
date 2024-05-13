using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IFineTuningBertScoreMetricsDataService
{
    Task<FineTuningBertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<FineTuningBertScoreEvaluationMetric> CreateAsync(FineTuningBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<FineTuningBertScoreEvaluationMetric> GetAll();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<FineTuningBertScoreEvaluationMetric> UpdateAsync(int id, FineTuningBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
}
