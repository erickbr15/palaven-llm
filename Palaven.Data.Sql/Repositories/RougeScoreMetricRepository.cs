using Liara.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class RougeScoreMetricRepository : GenericRepository<RougeScoreMetric>
{
    public RougeScoreMetricRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
