using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Services;

public class PerformanceMetricsDataService : IPerformanceMetricsDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<BertScoreMetric> _bertScoreMetricRepository;
    private readonly IRepository<RougeScoreMetric> _rougeScoreMetricRepository;

    public PerformanceMetricsDataService(PalavenDbContext dbContext,
        IRepository<BertScoreMetric> bertScoreMetricRepository, 
        IRepository<RougeScoreMetric> rougeScoreMetricRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _bertScoreMetricRepository = bertScoreMetricRepository ?? throw new ArgumentNullException(nameof(bertScoreMetricRepository));
        _rougeScoreMetricRepository = rougeScoreMetricRepository ?? throw new ArgumentNullException(nameof(rougeScoreMetricRepository));
    }

    public IEnumerable<BertScoreMetric> FetchBertScoreMetrics(Func<BertScoreMetric, bool> criteria)
    {
        return _bertScoreMetricRepository.GetAll()
            .Where(criteria)
            .ToList();
    }    

    public IEnumerable<RougeScoreMetric> FetchRougeScoreMetrics(Func<RougeScoreMetric, bool> criteria)
    {
        return _rougeScoreMetricRepository.GetAll()
            .Where(criteria)
            .ToList();
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken)
    {

        var existingEvaluation = _bertScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == bertScoreMetrics.SessionId &&
                                x.BatchNumber == bertScoreMetrics.BatchNumber &&
                                x.EvaluationExerciseId == bertScoreMetrics.EvaluationExerciseId);

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
        foreach (var rougeScoreMetric in rougeScoreMetrics)
        {
            await UpsertChatCompletionPerformanceEvaluationAsync(rougeScoreMetric, cancellationToken);
        }
    }

    private async Task UpsertChatCompletionPerformanceEvaluationAsync(RougeScoreMetric rougeScoreMetric, CancellationToken cancellationToken)
    {
        var existingEvaluation = _rougeScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.SessionId == rougeScoreMetric.SessionId && 
                    x.BatchNumber == rougeScoreMetric.BatchNumber && 
                    x.EvaluationExerciseId == rougeScoreMetric.EvaluationExerciseId &&
                    string.Equals(x.RougeType, rougeScoreMetric.RougeType, StringComparison.OrdinalIgnoreCase));

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

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
