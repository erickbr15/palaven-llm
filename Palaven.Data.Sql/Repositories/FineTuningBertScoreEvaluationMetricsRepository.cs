using Liara.EntityFrameworkCore;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Repositories;

public class FineTuningBertScoreEvaluationMetricsRepository : GenericRepository<FineTuningBertScoreEvaluationMetric>
{
    public FineTuningBertScoreEvaluationMetricsRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
