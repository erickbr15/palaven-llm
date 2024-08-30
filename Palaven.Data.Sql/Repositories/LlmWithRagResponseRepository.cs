using Liara.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class LlmWithRagResponseRepository : GenericRepository<LlmWithRagResponse>
{
    public LlmWithRagResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}    
