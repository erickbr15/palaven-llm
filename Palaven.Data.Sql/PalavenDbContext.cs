using Microsoft.EntityFrameworkCore;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql;

public class PalavenDbContext : DbContext, IPalavenDbContext
{
    public PalavenDbContext(DbContextOptions options)
        :base(options)
    {
    }

    public DbSet<InstructionEntity> Instructions { get; set; } = default!;
    public DbSet<EvaluationSession> EvaluationSessions { get; set; } = default!;
    public DbSet<FineTunedLlmResponse> FineTunedLlmResponses { get; set; } = default!;
    public DbSet<FineTunedLlmWithRagResponse> FineTunedLlmWithRagResponses { get; set; } = default!;
    public DbSet<LlmResponse> LlmResponses { get; set; } = default!;
    public DbSet<LlmWithRagResponse> LlmWithRagResponses { get; set; } = default!;
    public DbSet<BertScoreMetric> BertScoreMetrics { get; set; } = default!;
    public DbSet<RougeScoreMetric> RougeScoreMetrics { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PalavenDbContext).Assembly);
    }
}
