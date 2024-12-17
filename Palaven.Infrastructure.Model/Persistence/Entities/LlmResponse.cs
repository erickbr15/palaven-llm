namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class LlmResponse
{    
    public Guid EvaluationSessionId { get; set; }
    public Guid InstructionId { get; set; }
    public int EvaluationExerciseId { get; set; }
    public int BatchNumber { get; set; }        
    public string? Response { get; set; }
    public string? CleanResponse { get; set; }
    public float ElapsedTime { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public InstructionEntity Instruction { get; set; } = default!;
    public EvaluationSession EvaluationSession { get; set; } = default!;
    public EvaluationExercise EvaluationExercise { get; set; } = default!;
}
