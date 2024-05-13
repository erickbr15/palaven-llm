using Liara.EntityFrameworkCore;
using Palaven.Model.Datasets;

namespace Palaven.Data.Sql.Repositories;

public class InstructionRepository : GenericRepository<Instruction>
{
    public InstructionRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
