using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class BertScoreMetricTypeConfiguration : IEntityTypeConfiguration<BertScoreMetric>
{
    public void Configure(EntityTypeBuilder<BertScoreMetric> builder)
    {
        builder.ToTable("BertScoreMetrics", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        
        builder.Property(x=> x.EvaluationSessionId).IsRequired();
        builder.Property(x => x.EvaluationExerciseId).IsRequired();
        builder.Property(x=> x.BatchNumber).IsRequired();
        builder.Property(x => x.Precision).HasColumnType("float");
        builder.Property(x => x.Recall).HasColumnType("float");
        builder.Property(x => x.F1).HasColumnType("float");        

        builder.Property(x => x.CreationDate).IsRequired();
        builder.Property(x => x.ModifiedDate);

        builder.HasOne(x => x.EvaluationExercise)
            .WithMany()
            .HasForeignKey(x => x.EvaluationExerciseId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.EvaluationSessionId)
            .IsRequired(true);

        builder.Navigation(builder => builder.EvaluationExercise).AutoInclude(true);
        builder.Navigation(builder => builder.EvaluationSession).AutoInclude(true);
    }
}
