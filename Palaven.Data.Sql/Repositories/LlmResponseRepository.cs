using Liara.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class LlmResponseRepository : GenericRepository<LlmResponse>
{
    public LlmResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
