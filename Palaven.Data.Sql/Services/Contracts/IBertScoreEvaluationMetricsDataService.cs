using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IBertScoreEvaluationMetricsDataService
{
    Task<BertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<BertScoreEvaluationMetric> CreateAsync(BertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<BertScoreEvaluationMetric> GetAll();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<BertScoreEvaluationMetric> UpdateAsync(int id, BertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
}
