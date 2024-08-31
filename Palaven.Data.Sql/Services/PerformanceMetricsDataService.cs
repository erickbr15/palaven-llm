using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Services;

public class PerformanceMetricsDataService : IPerformanceMetricsDataService
{
    private readonly IRepository<BertScoreMetric> _bertScoreMetricRepository;
    private readonly IRepository<RougeScoreMetric> _rougeScoreMetricRepository;

    public PerformanceMetricsDataService(IRepository<BertScoreMetric> bertScoreMetricRepository, IRepository<RougeScoreMetric> rougeScoreMetricRepository)
    {
        _bertScoreMetricRepository = bertScoreMetricRepository ?? throw new ArgumentNullException(nameof(bertScoreMetricRepository));
        _rougeScoreMetricRepository = rougeScoreMetricRepository ?? throw new ArgumentNullException(nameof(rougeScoreMetricRepository));
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken)
    {
        var existingEvaluation = _bertScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == bertScoreMetrics.SessionId && x.BatchNumber == bertScoreMetrics.BatchNumber);

        if (existingEvaluation == null)
        {
            bertScoreMetrics.CreationDate = DateTime.Now;
            await _bertScoreMetricRepository.AddAsync(bertScoreMetrics, cancellationToken);
        }
        else
        {
            existingEvaluation.BertScorePrecision = bertScoreMetrics.BertScorePrecision;
            existingEvaluation.BertScoreRecall = bertScoreMetrics.BertScoreRecall;
            existingEvaluation.BertScoreF1 = bertScoreMetrics.BertScoreF1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _bertScoreMetricRepository.Update(existingEvaluation);
        }
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(IEnumerable<RougeScoreMetric> rougeScoreMetrics, CancellationToken cancellationToken)
    {
        foreach (var rougeScoreMetric in rougeScoreMetrics.ToList())
        {
            await UpsertChatCompletionPerformanceEvaluationAsync(rougeScoreMetric, cancellationToken);
        }
    }

    private async Task UpsertChatCompletionPerformanceEvaluationAsync(RougeScoreMetric rougeScoreMetric, CancellationToken cancellationToken)
    {
        var existingEvaluation = _rougeScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == rougeScoreMetric.SessionId && x.BatchNumber == rougeScoreMetric.BatchNumber && string.Equals(x.RougeType, rougeScoreMetric.RougeType, StringComparison.OrdinalIgnoreCase));

        if (existingEvaluation == null)
        {
            rougeScoreMetric.CreationDate = DateTime.Now;
            await _rougeScoreMetricRepository.AddAsync(rougeScoreMetric, cancellationToken);
        }
        else
        {
            existingEvaluation.RougePrecision = rougeScoreMetric.RougePrecision;
            existingEvaluation.RougeRecall = rougeScoreMetric.RougeRecall;
            existingEvaluation.RougeF1 = rougeScoreMetric.RougeF1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _rougeScoreMetricRepository.Update(existingEvaluation);
        }
    }
}
