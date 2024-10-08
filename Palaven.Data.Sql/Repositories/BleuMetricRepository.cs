using Liara.EntityFrameworkCore;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.Repositories;

public class BleuMetricRepository : GenericRepository<BleuMetric>
{
    public BleuMetricRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {

    }
}
