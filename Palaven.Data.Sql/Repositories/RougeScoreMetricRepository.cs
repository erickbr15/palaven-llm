using Liara.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class RougeScoreMetricRepository : GenericRepository<RougeScoreMetric>
{
    public RougeScoreMetricRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
