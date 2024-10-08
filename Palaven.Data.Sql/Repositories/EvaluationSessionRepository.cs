using Liara.EntityFrameworkCore;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.Repositories;

public class EvaluationSessionRepository : GenericRepository<EvaluationSession>
{
    public EvaluationSessionRepository(PalavenDbContext dbContext) 
        : base(dbContext)
    {
    }
}
