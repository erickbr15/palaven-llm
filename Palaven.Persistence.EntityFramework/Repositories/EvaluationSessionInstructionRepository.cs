using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class EvaluationSessionInstructionRepository : GenericRepository<EvaluationSessionInstruction>
{
    public EvaluationSessionInstructionRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
