using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.EntityMappings;

public class BertScoreMetricTypeConfiguration : IEntityTypeConfiguration<BertScoreMetric>
{
    public void Configure(EntityTypeBuilder<BertScoreMetric> builder)
    {
        builder.ToTable("BertScoreMetrics", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        
        builder.Property(x=> x.SessionId).IsRequired();
        builder.Property(x => x.EvaluationExerciseId).IsRequired();
        builder.Property(x=> x.BatchNumber).IsRequired();
        builder.Property(x => x.BertScorePrecision).HasColumnName("BertScorePrecision").HasColumnType("float");
        builder.Property(x => x.BertScoreRecall).HasColumnName("BertScoreRecall").HasColumnType("float");
        builder.Property(x => x.BertScoreF1).HasColumnName("BertScoreF1").HasColumnType("float");        

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
