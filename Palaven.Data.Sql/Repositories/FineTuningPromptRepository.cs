using Liara.EntityFrameworkCore;
using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Repositories;

public class FineTuningPromptRepository : GenericRepository<FineTuningPromptEntity>
{
    public FineTuningPromptRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
