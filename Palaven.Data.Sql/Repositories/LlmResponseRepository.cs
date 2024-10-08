using Liara.EntityFrameworkCore;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.Repositories;

public class LlmResponseRepository : GenericRepository<LlmResponse>
{
    public LlmResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
