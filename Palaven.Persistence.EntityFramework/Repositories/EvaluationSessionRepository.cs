using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.Repositories;

public class EvaluationSessionRepository : GenericRepository<EvaluationSession>
{
    public EvaluationSessionRepository(PalavenDbContext dbContext) : base(dbContext)
    {
    }
}
