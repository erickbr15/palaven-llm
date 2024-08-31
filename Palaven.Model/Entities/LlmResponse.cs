namespace Palaven.Model.Entities;

public class LlmResponse
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int BatchNumber { get; set; }
    public int EvaluationExerciseId { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public string? LlmResponseToEvaluate { get; set; }
    public float ElapsedTime { get; set; }    
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public InstructionEntity Instruction { get; set; } = default!;
    public EvaluationSession EvaluationSession { get; set; } = default!;
    public EvaluationExercise EvaluationExercise { get; set; } = default!;
}
