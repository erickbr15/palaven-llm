using Liara.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class BertScoreMetricRepository : GenericRepository<BertScoreMetric>
{
    public BertScoreMetricRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
