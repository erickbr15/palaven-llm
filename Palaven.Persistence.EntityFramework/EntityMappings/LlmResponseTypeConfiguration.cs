using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class LlmResponseTypeConfiguration : IEntityTypeConfiguration<LlmResponse>
{
    public void Configure(EntityTypeBuilder<LlmResponse> builder)
    {
        builder.ToTable("LlmResponses", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => new { x.EvaluationSessionId, x.InstructionId, x.EvaluationExerciseId });
        
        builder.Property(x => x.EvaluationSessionId).IsRequired();
        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x => x.EvaluationExerciseId).IsRequired();

        builder.Property(x => x.BatchNumber).IsRequired();
        
        builder.Property(x => x.Response).HasColumnName("LlmResponse")
            .HasColumnType("text");

        builder.Property(x => x.CleanResponse).HasColumnName("LlmCleanResponse")
            .HasColumnType("text");

        builder.Property(x => x.ElapsedTime).HasColumnType("float").IsRequired();

        builder.Property(x=> x.CreationDate).IsRequired();
        builder.Property(x=> x.ModifiedDate);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.EvaluationSessionId)
            .IsRequired(true);

        builder.HasOne(x => x.Instruction)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationExercise)
            .WithMany()
            .HasForeignKey(x => x.EvaluationExerciseId)
            .IsRequired(true);

        builder.Navigation(x => x.EvaluationSession).AutoInclude();
        builder.Navigation(x => x.Instruction).AutoInclude();
        builder.Navigation(x => x.EvaluationExercise).AutoInclude();
    }
}
