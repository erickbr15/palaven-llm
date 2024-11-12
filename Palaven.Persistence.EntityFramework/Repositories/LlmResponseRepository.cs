using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class LlmResponseRepository : GenericRepository<LlmResponse>
{
    public LlmResponseRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
