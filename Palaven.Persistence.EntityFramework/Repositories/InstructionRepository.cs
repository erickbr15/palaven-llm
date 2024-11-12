using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class InstructionRepository : GenericRepository<InstructionEntity>
{
    public InstructionRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
