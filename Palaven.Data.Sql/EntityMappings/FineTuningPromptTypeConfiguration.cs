using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.EntityMappings;

public class FineTuningPromptTypeConfiguration : IEntityTypeConfiguration<FineTuningPromptEntity>
{
    public void Configure(EntityTypeBuilder<FineTuningPromptEntity> builder)
    {
        builder.ToTable("FineTuningPrompts", PalavenDbSchemas.Datasets);

        builder.HasKey(b => b.PromptId);
        builder.Property(b => b.PromptId).HasColumnName("Id").IsRequired().UseIdentityColumn();

        builder.Property(b => b.InstructionId).IsRequired();
        builder.Property(b => b.DatasetId).IsRequired();
        builder.Property(b => b.LargeLanguageModel).HasMaxLength(2000).IsRequired();
        builder.Property(b => b.Prompt).IsRequired();

        builder.HasOne(b => b.Instruction)
            .WithMany()
            .HasForeignKey(b => b.InstructionId)
            .IsRequired(true);

        builder.Navigation(b => b.Instruction).AutoInclude(true);
    }
}
