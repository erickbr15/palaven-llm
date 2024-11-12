using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class InstructionTypeConfiguration : IEntityTypeConfiguration<InstructionEntity>
{
    public void Configure(EntityTypeBuilder<InstructionEntity> builder)
    {
        builder.ToTable("Instructions", PalavenDbSchemas.Datasets);

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityColumn();

        builder.Property(x => x.InstructionId).IsRequired();
        builder.Property(x=> x.DatasetId).IsRequired();
        builder.Property(x => x.Instruction)
            .HasColumnName("Instruction")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Response)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x=>x.Category)
            .HasColumnName("Category")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.GoldenArticleId).IsRequired();
    }
}
