﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Palaven.Infrastructure.Model.Persistence.Entities;

namespace Palaven.Persistence.EntityFramework.EntityMappings;

public class EvaluationSessionTypeConfiguration : IEntityTypeConfiguration<EvaluationSession>
{
    public void Configure(EntityTypeBuilder<EvaluationSession> builder)
    {
        builder.ToTable("EvaluationSessions", PalavenDbSchemas.LlmPerformanceEvaluation);

        builder.HasKey(x => x.SessionId);
        
        builder.Property(x=> x.SessionId).IsRequired();
        builder.Property(x => x.DatasetId).IsRequired();
        builder.Property(x => x.BatchSize).IsRequired();
        builder.Property(x=> x.LargeLanguageModel).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.DeviceInfo).HasMaxLength(250).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate);
        builder.Property(x => x.CreationDate).IsRequired();
        builder.Property(x => x.ModifiedDate);
    }
}
