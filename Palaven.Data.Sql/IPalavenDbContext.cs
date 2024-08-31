using Microsoft.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql;

public interface IPalavenDbContext
{
    DbSet<InstructionEntity> Instructions { get; set; }
    DbSet<EvaluationSession> EvaluationSessions { get; set; }
    DbSet<EvaluationSessionInstruction> EvaluationSessionInstructions { get; set; }
    DbSet<FineTunedLlmResponse> FineTunedLlmResponses { get; set; }
    DbSet<FineTunedLlmWithRagResponse> FineTunedLlmWithRagResponses { get; set; }
    DbSet<LlmResponse> LlmResponses { get; set; }
    DbSet<LlmWithRagResponse> LlmWithRagResponses { get; set; }
    DbSet<BertScoreMetric> BertScoreMetrics { get; set; }
    DbSet<RougeScoreMetric> RougeScoreMetrics { get; set; }    
}
