using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class FineTuningPromptTypeConfiguration : IEntityTypeConfiguration<FineTuningPromptEntity>
{
    public void Configure(EntityTypeBuilder<FineTuningPromptEntity> builder)
    {
        builder.ToTable("FineTuningPrompts", PalavenDbSchemas.Datasets);

        builder.HasKey(b => b.PromptId);
        builder.Property(b => b.PromptId).IsRequired();

        builder.Property(b => b.InstructionId).IsRequired();
        builder.Property(b => b.LargeLanguageModel).HasMaxLength(2000).IsRequired();
        builder.Property(b => b.Prompt).IsRequired();

        builder.HasOne(b => b.Instruction)
            .WithMany()
            .HasForeignKey(b => b.InstructionId)
            .IsRequired(true);

        builder.Navigation(b => b.Instruction).AutoInclude(true);
    }
}
