using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class LlmResponseTypeConfiguration : IEntityTypeConfiguration<LlmResponse>
{
    public void Configure(EntityTypeBuilder<LlmResponse> builder)
    {
        builder.ToTable("LlmResponses", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.EvaluationExerciseId).IsRequired();
        builder.Property(x => x.BatchNumber).IsRequired();
        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x => x.ResponseCompletion)
            .HasColumnName("LlmResponseCompletion")
            .HasColumnType("text");

        builder.Property(x => x.LlmResponseToEvaluate)
            .HasColumnType("text");

        builder.Property(x => x.ElapsedTime).HasColumnType("float").IsRequired();

        builder.Property(x=> x.CreationDate).IsRequired();
        builder.Property(x=> x.ModifiedDate);

        builder.HasOne(x => x.EvaluationExercise)
            .WithMany()
            .HasForeignKey(x => x.EvaluationExerciseId)
            .IsRequired(true);

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
        builder.Navigation(x => x.EvaluationExercise).AutoInclude();
    }
}
