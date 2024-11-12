using Microsoft.EntityFrameworkCore;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework;

public interface IPalavenDbContext
{
    DbSet<InstructionEntity> Instructions { get; set; }
    DbSet<EvaluationSession> EvaluationSessions { get; set; }
    DbSet<EvaluationSessionInstruction> EvaluationSessionInstructions { get; set; }
    DbSet<LlmResponse> LlmResponses { get; set; }
    DbSet<BertScoreMetric> BertScoreMetrics { get; set; }
    DbSet<RougeScoreMetric> RougeScoreMetrics { get; set; }
    DbSet<BleuMetric> BleuMetrics { get; set; }
}
