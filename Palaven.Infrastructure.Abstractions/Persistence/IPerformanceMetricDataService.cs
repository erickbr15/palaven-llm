using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Infrastructure.Abstractions.Persistence;

public interface IPerformanceMetricDataService
{
    IEnumerable<BertScoreMetric> FetchBertScoreMetrics(Func<BertScoreMetric, bool> criteria);
    IEnumerable<RougeScoreMetric> FetchRougeScoreMetrics(Func<RougeScoreMetric, bool> criteria);
    IEnumerable<BleuMetric> FetchBleuMetrics(Func<BleuMetric, bool> criteria);
    Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken);
    Task UpsertChatCompletionPerformanceEvaluationAsync(BleuMetric bleuMetrics, CancellationToken cancellationToken);
    Task UpsertChatCompletionPerformanceEvaluationAsync(IEnumerable<RougeScoreMetric> rougeScoreMetrics, CancellationToken cancellationToken);
    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
