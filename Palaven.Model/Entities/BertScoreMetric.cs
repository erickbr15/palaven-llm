namespace Palaven.Model.Entities;

public class BertScoreMetric
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int EvaluationExerciseId { get; set; }
    public int BatchNumber { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }    
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public EvaluationSession EvaluationSession { get; set; } = default!;
    public EvaluationExercise EvaluationExercise { get; set; } = default!;
}
