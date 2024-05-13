using Microsoft.EntityFrameworkCore;
using Palaven.Model.Datasets;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql;

public class PalavenDbContext : DbContext, IPalavenDbContext
{
    public PalavenDbContext(DbContextOptions options)
        :base(options)
    {
    }

    public DbSet<Instruction> Instructions { get; set; } = default!;
    public DbSet<BertScoreEvaluationMetric> BertScoreEvaluationMetrics { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PalavenDbContext).Assembly);
    }
}
