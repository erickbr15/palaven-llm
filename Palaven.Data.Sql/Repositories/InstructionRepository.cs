using Liara.EntityFrameworkCore;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.Repositories;

public class InstructionRepository : GenericRepository<InstructionEntity>
{
    public InstructionRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
