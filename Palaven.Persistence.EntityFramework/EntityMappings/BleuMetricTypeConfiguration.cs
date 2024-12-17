using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class BleuMetricTypeConfiguration : IEntityTypeConfiguration<BleuMetric>
{
    public void Configure(EntityTypeBuilder<BleuMetric> builder)
    {
        builder.ToTable("BleuMetrics", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(e => e.Id);
        builder.Property(e=> e.Id).IsRequired().UseIdentityColumn();

        builder.Property(e => e.EvaluationSessionId).IsRequired();
        builder.Property(e => e.EvaluationExerciseId).IsRequired();
        builder.Property(e => e.BatchNumber).IsRequired();
        builder.Property(e => e.Score).HasColumnType("float");
        builder.Property(e => e.CreationDate).IsRequired();
        builder.Property(e => e.ModifiedDate);

        builder.HasOne(e => e.EvaluationSession)
            .WithMany()
            .HasForeignKey(e => e.EvaluationSessionId)
            .IsRequired(true);

        builder.HasOne(e => e.EvaluationExercise)
            .WithMany()
            .HasForeignKey(e => e.EvaluationExerciseId)
            .IsRequired(true);

        builder.Navigation(e => e.EvaluationSession).AutoInclude(true);
        builder.Navigation(e => e.EvaluationExercise).AutoInclude(true);
    }
}
