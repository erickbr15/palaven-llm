﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class EvaluationSessionInstructionTypeConfiguration : IEntityTypeConfiguration<EvaluationSessionInstruction>
{
    public void Configure(EntityTypeBuilder<EvaluationSessionInstruction> builder)
    {
        builder.ToTable("EvaluationSessionInstructions", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).IsRequired().UseIdentityColumn();
        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x => x.EvaluationSessionId).IsRequired();
        builder.Property(x => x.InstructionPurpose)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasOne(x => x.Instruction)
            .WithMany()
            .HasForeignKey(x => x.InstructionId)
            .IsRequired(true);

        builder.HasOne(x => x.EvaluationSession)
            .WithMany()
            .HasForeignKey(x => x.EvaluationSessionId)
            .IsRequired(true);

        builder.Navigation(builder => builder.Instruction).AutoInclude(true);
        builder.Navigation(builder => builder.EvaluationSession).AutoInclude(true);
    }
}
