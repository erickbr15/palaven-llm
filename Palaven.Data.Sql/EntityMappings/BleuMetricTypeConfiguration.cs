using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.EntityMappings;

public class BleuMetricTypeConfiguration : IEntityTypeConfiguration<BleuMetric>
{
    public void Configure(EntityTypeBuilder<BleuMetric> builder)
    {
        builder.ToTable("BleuMetrics", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(e => e.Id);
        builder.Property(e=> e.Id).IsRequired().UseIdentityColumn();
        builder.Property(e => e.SessionId).IsRequired();
        builder.Property(e => e.EvaluationExerciseId).IsRequired();
        builder.Property(e => e.BatchNumber).IsRequired();
        builder.Property(e => e.BleuScore).HasColumnType("float");
        builder.Property(e => e.CreationDate).IsRequired();
        builder.Property(e => e.ModifiedDate);

        builder.HasOne(e => e.EvaluationSession)
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .IsRequired(true);

        builder.HasOne(e => e.EvaluationExercise)
            .WithMany()
            .HasForeignKey(e => e.EvaluationExerciseId)
            .IsRequired(true);

        builder.Navigation(e => e.EvaluationSession).AutoInclude(true);
        builder.Navigation(e => e.EvaluationExercise).AutoInclude(true);
    }
}
