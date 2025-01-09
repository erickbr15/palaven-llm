using Liara.Common.Abstractions.Persistence;
using Palaven.Infrastructure.Abstractions.Persistence;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Services;

public class PerformanceMetricsDataService : IPerformanceMetricDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<BertScoreMetric> _bertScoreMetricRepository;
    private readonly IRepository<RougeScoreMetric> _rougeScoreMetricRepository;
    private readonly IRepository<BleuMetric> _bleuMetricRepository;

    public PerformanceMetricsDataService(PalavenDbContext dbContext,
        IRepository<BertScoreMetric> bertScoreMetricRepository, 
        IRepository<RougeScoreMetric> rougeScoreMetricRepository,
        IRepository<BleuMetric> bleuMetricRepository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _bertScoreMetricRepository = bertScoreMetricRepository ?? throw new ArgumentNullException(nameof(bertScoreMetricRepository));
        _rougeScoreMetricRepository = rougeScoreMetricRepository ?? throw new ArgumentNullException(nameof(rougeScoreMetricRepository));
        _bleuMetricRepository = bleuMetricRepository ?? throw new ArgumentNullException(nameof(bleuMetricRepository));
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

    public IEnumerable<BleuMetric> FetchBleuMetrics(Func<BleuMetric, bool> criteria)
    {
        return _bleuMetricRepository.GetAll()
            .Where(criteria)
            .ToList();
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BertScoreMetric bertScoreMetrics, CancellationToken cancellationToken)
    {

        var existingEvaluation = _bertScoreMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.EvaluationSessionId == bertScoreMetrics.EvaluationSessionId &&
                                x.BatchNumber == bertScoreMetrics.BatchNumber &&
                                x.EvaluationExerciseId == bertScoreMetrics.EvaluationExerciseId);

        if (existingEvaluation == null)
        {
            bertScoreMetrics.CreationDate = DateTime.Now;
            await _bertScoreMetricRepository.AddAsync(bertScoreMetrics, cancellationToken);
        }
        else
        {
            existingEvaluation.Precision = bertScoreMetrics.Precision;
            existingEvaluation.Recall = bertScoreMetrics.Recall;
            existingEvaluation.F1 = bertScoreMetrics.F1;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _bertScoreMetricRepository.Update(existingEvaluation);
        }
    }

    public async Task UpsertChatCompletionPerformanceEvaluationAsync(BleuMetric bleuMetrics, CancellationToken cancellationToken)
    {
        var existingEvaluation = _bleuMetricRepository
            .GetAll()
            .SingleOrDefault(x => x.EvaluationSessionId == bleuMetrics.EvaluationSessionId &&
                                x.BatchNumber == bleuMetrics.BatchNumber &&
                                x.EvaluationExerciseId == bleuMetrics.EvaluationExerciseId);

        if (existingEvaluation == null)
        {
            bleuMetrics.CreationDate = DateTime.Now;
            await _bleuMetricRepository.AddAsync(bleuMetrics, cancellationToken);
        }
        else
        {
            existingEvaluation.Score = bleuMetrics.Score;
            existingEvaluation.ModifiedDate = DateTime.Now;

            _bleuMetricRepository.Update(existingEvaluation);
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
            .SingleOrDefault(x => x.EvaluationSessionId == rougeScoreMetric.EvaluationSessionId && 
                    x.BatchNumber == rougeScoreMetric.BatchNumber && 
                    x.EvaluationExerciseId == rougeScoreMetric.EvaluationExerciseId &&
                    x.RougeType == rougeScoreMetric.RougeType);

        if (existingEvaluation == null)
        {
            rougeScoreMetric.CreationDate = DateTime.Now;
            await _rougeScoreMetricRepository.AddAsync(rougeScoreMetric, cancellationToken);
        }
        else
        {
            existingEvaluation.Precision = rougeScoreMetric.Precision;
            existingEvaluation.Recall = rougeScoreMetric.Recall;
            existingEvaluation.F1 = rougeScoreMetric.F1;
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
