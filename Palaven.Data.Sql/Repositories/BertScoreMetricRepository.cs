using Liara.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class BertScoreMetricRepository : GenericRepository<BertScoreMetric>
{
    public BertScoreMetricRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
