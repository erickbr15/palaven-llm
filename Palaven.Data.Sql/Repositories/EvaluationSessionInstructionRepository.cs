using Liara.EntityFrameworkCore;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.Repositories;

public class EvaluationSessionInstructionRepository : GenericRepository<EvaluationSessionInstruction>
{
    public EvaluationSessionInstructionRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
