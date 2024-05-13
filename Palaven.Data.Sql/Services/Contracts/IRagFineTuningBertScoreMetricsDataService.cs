using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IRagFineTuningBertScoreMetricsDataService
{
    Task<RagFineTuningBertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<RagFineTuningBertScoreEvaluationMetric> CreateAsync(RagFineTuningBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    IList<RagFineTuningBertScoreEvaluationMetric> GetAll();
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task<RagFineTuningBertScoreEvaluationMetric> UpdateAsync(int id, RagFineTuningBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken);
}
