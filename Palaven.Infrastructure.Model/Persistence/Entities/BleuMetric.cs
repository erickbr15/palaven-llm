﻿namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class BleuMetric
{
    public int Id { get; set; }
    public Guid EvaluationSessionId { get; set; }
    public int EvaluationExerciseId { get; set; }
    public int BatchNumber { get; set; }
    public float? Score { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public EvaluationSession EvaluationSession { get; set; } = default!;
    public EvaluationExercise EvaluationExercise { get; set; } = default!;

}
