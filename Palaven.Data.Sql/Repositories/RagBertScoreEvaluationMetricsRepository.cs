using Liara.EntityFrameworkCore;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Repositories;

public class RagBertScoreEvaluationMetricsRepository : GenericRepository<RagBertScoreEvaluationMetric>
{
    public RagBertScoreEvaluationMetricsRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
