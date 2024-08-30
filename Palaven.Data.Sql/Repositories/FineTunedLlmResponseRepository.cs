using Liara.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class FineTunedLlmResponseRepository : GenericRepository<FineTunedLlmResponse>
{
    public FineTunedLlmResponseRepository(PalavenDbContext dbContext)
        : base(dbContext)
    {
    }
}
