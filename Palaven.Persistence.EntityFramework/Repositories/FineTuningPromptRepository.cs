using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class FineTuningPromptRepository : GenericRepository<FineTuningPromptEntity>
{
    public FineTuningPromptRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
