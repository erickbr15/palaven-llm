using Liara.EntityFrameworkCore;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Repositories;

public class BertScoreEvaluationMetricsRepository : GenericRepository<BertScoreEvaluationMetric>
{
    public BertScoreEvaluationMetricsRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
