using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class RougeScoreMetricRepository : GenericRepository<RougeScoreMetric>
{
    public RougeScoreMetricRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
