using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class BertScoreMetricRepository : GenericRepository<BertScoreMetric>
{
    public BertScoreMetricRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
