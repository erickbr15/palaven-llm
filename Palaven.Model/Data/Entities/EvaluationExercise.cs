namespace Palaven.Model.Data.Entities;

public class EvaluationExercise
{
    public int Id { get; set; }
    public string Exercise { get; set; } = default!;
    public string? Description { get; set; }
}
