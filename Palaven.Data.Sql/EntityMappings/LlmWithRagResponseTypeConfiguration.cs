using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.EntityMappings;

public class LlmWithRagResponseTypeConfiguration : IEntityTypeConfiguration<LlmWithRagResponse>
{
    public void Configure(EntityTypeBuilder<LlmWithRagResponse> builder)
    {
        builder.ToTable("LlmWithRagResponses", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.BatchNumber).IsRequired();
        builder.Property(x=> x.InstructionId).IsRequired();
        builder.Property(x => x.ResponseCompletion)
            .HasColumnName("LlmResponseCompletion")
            .HasColumnType("text");
        builder.Property(x => x.ElapsedTime).HasColumnType("float").IsRequired();
        builder.Property(x=> x.CreationDate).IsRequired();
        builder.Property(x=> x.ModifiedDate);

        builder.HasOne(builder => builder.Instruction)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.HasOne(builder => builder.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .IsRequired(true);

        builder.Navigation(builder => builder.Instruction).AutoInclude(true);
        builder.Navigation(builder => builder.EvaluationSession).AutoInclude(true);
    }
}
