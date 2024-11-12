using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class BleuMetricRepository : GenericRepository<BleuMetric>
{
    public BleuMetricRepository(PalavenDbContext dbContext) : base(dbContext)
    {

    }
}
