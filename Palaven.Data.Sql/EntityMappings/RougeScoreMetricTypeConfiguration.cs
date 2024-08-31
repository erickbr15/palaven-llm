using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.Entities;

namespace Palaven.Data.Sql.EntityMappings;

public class RougeScoreMetricTypeConfiguration : IEntityTypeConfiguration<RougeScoreMetric>
{
    public void Configure(EntityTypeBuilder<RougeScoreMetric> builder)
    {
        builder.ToTable("RougeScoreMetrics", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.EvaluationExerciseId).IsRequired();

        builder.Property(x => x.BatchNumber).IsRequired();
        builder.Property( x=> x.RougeType).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RougePrecision).HasColumnType("float");
        builder.Property(x => x.RougeRecall).HasColumnType("float");
        builder.Property(x => x.RougeF1).HasColumnName("RougeScoreF1").HasColumnType("float");

        builder.Property(x => x.CreationDate).IsRequired();
        builder.Property(x => x.ModifiedDate);

        builder.HasOne(x => x.EvaluationExercise)
            .WithMany()
            .HasForeignKey(x => x.EvaluationExerciseId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .IsRequired(true);

        builder.Navigation(builder => builder.EvaluationExercise).AutoInclude(true);
        builder.Navigation(builder => builder.EvaluationSession).AutoInclude(true);
    }
}
