using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.PerformanceEvaluation;

namespace Palaven.Data.Sql.EntityMappings;

public class FineTunedLlmResponseTypeConfiguration : IEntityTypeConfiguration<FineTunedLlmResponse>
{
    public void Configure(EntityTypeBuilder<FineTunedLlmResponse> builder)
    {
        builder.ToTable("FineTunedLlmResponses", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.BatchNumber).IsRequired();
        builder.Property(x => x.InstructionId).IsRequired();

        builder.Property(x=> x.ResponseCompletion)
            .HasColumnName("LlmResponseCompletion")
            .HasColumnType("text");
        
        builder.Property(x=> x.CreationDate).IsRequired();
        builder.Property(x=> x.ModifiedDate);

        builder.HasOne(x => x.Instruction)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .IsRequired(true);

        builder.Navigation(builder => builder.Instruction).AutoInclude(true);
        builder.Navigation(builder => builder.EvaluationSession).AutoInclude(true);
    }
}
