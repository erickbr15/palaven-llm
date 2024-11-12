namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class EvaluationSessionInstruction
{
    public long Id { get; set; }
    public int InstructionId { get; set; }
    public Guid EvaluationSessionId { get; set; }
    public string InstructionPurpose { get; set; } = default!;

    public InstructionEntity Instruction { get; set; } = default!;
    public EvaluationSession EvaluationSession { get; set; } = default!;
}
