using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.EntityMappings;

public class FineTunedLlmWithRagResponseTypeConfiguration : IEntityTypeConfiguration<FineTunedLlmWithRagResponse>
{
    public void Configure(EntityTypeBuilder<FineTunedLlmWithRagResponse> builder)
    {
        builder.ToTable("FineTunedLlmWithRagResponses", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.BatchNumber).IsRequired();

        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x => x.ResponseCompletion)
            .HasColumnName("LlmResponseCompletion")            
            .HasColumnType("text");

        builder.Property(x => x.LlmResponseToEvaluate)
            .HasColumnType("text");

        builder.Property(x => x.ElapsedTime).HasColumnType("float").IsRequired();

        builder.Property(x => x.CreationDate).IsRequired();
        builder.Property(x => x.ModifiedDate);

        builder.HasOne(x => x.Instruction)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .IsRequired(true);

        builder.Navigation(x => x.Instruction).AutoInclude();
        builder.Navigation(x => x.EvaluationSession).AutoInclude();
    }   
}
