using Liara.Common.DataAccess;
using Palaven.Data.Sql.Services.Contracts;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Services;

public class RagBertScoreMetricsDataService : IRagBertScoreMetricsDataService
{
    private readonly PalavenDbContext _dbContext;
    private readonly IRepository<RagBertScoreEvaluationMetric> _repository;

    public RagBertScoreMetricsDataService(PalavenDbContext dbContext, IRepository<RagBertScoreEvaluationMetric> repository)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<RagBertScoreEvaluationMetric?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<RagBertScoreEvaluationMetric> CreateAsync(RagBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken)
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

    public IList<RagBertScoreEvaluationMetric> GetAll()
    {
        return _repository.GetAll().ToList();
    }

    public IQueryable<RagBertScoreEvaluationMetric> GetQueryable()
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

    public async Task<RagBertScoreEvaluationMetric> UpdateAsync(int id, RagBertScoreEvaluationMetric evaluationMetrics, CancellationToken cancellationToken)
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
