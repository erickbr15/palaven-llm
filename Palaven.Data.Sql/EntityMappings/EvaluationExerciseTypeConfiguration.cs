using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Model.Data.Entities;

namespace Palaven.Data.Sql.EntityMappings;

public class EvaluationExerciseTypeConfiguration : IEntityTypeConfiguration<EvaluationExercise>
{
    public void Configure(EntityTypeBuilder<EvaluationExercise> builder)
    {
        builder.ToTable("EvaluationExercises", PalavenDbSchemas.LlmPerformanceEvaluation);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).IsRequired();
        builder.Property(x => x.Exercise).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(5000);
    }
}
