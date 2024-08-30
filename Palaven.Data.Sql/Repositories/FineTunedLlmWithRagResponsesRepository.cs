using Liara.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class FineTunedLlmWithRagResponsesRepository : GenericRepository<FineTunedLlmWithRagResponse>
{
    public FineTunedLlmWithRagResponsesRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
