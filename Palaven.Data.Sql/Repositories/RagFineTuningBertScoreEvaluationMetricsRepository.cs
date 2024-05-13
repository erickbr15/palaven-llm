using Liara.EntityFrameworkCore;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.Repositories;

public class RagFineTuningBertScoreEvaluationMetricsRepository : GenericRepository<RagFineTuningBertScoreEvaluationMetric>
{
    public RagFineTuningBertScoreEvaluationMetricsRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
