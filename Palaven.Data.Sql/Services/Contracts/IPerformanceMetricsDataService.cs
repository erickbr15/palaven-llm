using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Services.Contracts;

public interface IPerformanceMetricsDataService
{
    Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken);
    Task UpsertChatCompletionPerformanceEvaluationAsync(IEnumerable<RougeScoreMetric> rougeScoreMetrics, CancellationToken cancellationToken);
}
