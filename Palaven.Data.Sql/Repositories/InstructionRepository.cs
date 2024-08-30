using Liara.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.Repositories;

public class InstructionRepository : GenericRepository<InstructionEntity>
{
    public InstructionRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
