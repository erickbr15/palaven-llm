using Liara.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class FineTunedLlmWithRagResponsesRepository : GenericRepository<FineTunedLlmWithRagResponse>
{
    public FineTunedLlmWithRagResponsesRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
