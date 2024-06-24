using Liara.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class FineTunedLlmResponseRepository : GenericRepository<FineTunedLlmResponse>
{
    public FineTunedLlmResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
