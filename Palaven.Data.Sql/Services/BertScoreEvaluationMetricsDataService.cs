using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services;

public class BertScoreEvaluationMetricsDataService : IBertScoreEvaluationMetricsDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<BertScoreEvaluationMetric> _repository;

    public BertScoreEvaluationMetricsDataService(PalavenDbContext dbContext, IRepository<BertScoreEvaluationMetric> repository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<BertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<BertScoreEvaluationMetric> CreateAsync(BertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken)
    {
        await _repository.AddAsync(evaluationMetrics, cancellationToken);
        return evaluationMetrics;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var evaluationMetrics = await _repository.GetByIdAsync(id, cancellationToken);

        if (evaluationMetrics != null)
        {
            _repository.Delete(evaluationMetrics);
        }        
    }

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return _repository.ExistsAsync(id, cancellationToken);
    }

    public IList<BertScoreEvaluationMetric> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public IQueryable<BertScoreEvaluationMetric> GetQueryable()
    {
        return _repository.GetAll();
    }

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BertScoreEvaluationMetric> UpdateAsync(int id, BertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken) ?? throw new InvalidOperationException($"Unable to find the entity with id {id}.");

        entity.LargeLanguageModel = evaluationMetrics.LargeLanguageModel;
        entity.Language = evaluationMetrics.Language;
        entity.ResponseCompletion = evaluationMetrics.ResponseCompletion;
        entity.BertScorePrecision = evaluationMetrics.BertScorePrecision;
        entity.BertScoreRecall = evaluationMetrics.BertScoreRecall;
        entity.BertScoreF1 = evaluationMetrics.BertScoreF1;        

        return entity;
    }
}
