using Liara.EntityFrameworkCore;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.Repositories;

public class LlmWithRagResponseRepository : GenericRepository<LlmWithRagResponse>
{
    public LlmWithRagResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}    
