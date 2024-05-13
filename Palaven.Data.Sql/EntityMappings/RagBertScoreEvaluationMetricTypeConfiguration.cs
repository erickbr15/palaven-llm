using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.LllmPerformance;

namespace Palaven.Data.Sql.EntityMappings;

public class RagBertScoreEvaluationMetricTypeConfiguration : IEntityTypeConfiguration<RagBertScoreEvaluationMetric>
{
    public void Configure(EntityTypeBuilder<RagBertScoreEvaluationMetric> builder)
    {
        builder.ToTable("RagBertScoreEvaluationMetrics", PalavenDbSchemas.Datasets);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();
        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x => x.LargeLanguageModel).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(10);

        builder.Property(x => x.BertScorePrecision).HasColumnName("BertScorePrecision").HasColumnType("float");
        builder.Property(x => x.BertScoreRecall).HasColumnName("BertScoreRecall").HasColumnType("float");
        builder.Property(x => x.BertScoreF1).HasColumnName("BertScoreF1").HasColumnType("float");

        builder.HasOne(x => x.InstructionRequest)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.Navigation(x => x.InstructionRequest).AutoInclude();
    }
}
