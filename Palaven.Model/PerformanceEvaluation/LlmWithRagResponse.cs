using Palaven.Model.Datasets;

namespace Palaven.Model.PerformanceEvaluation;

public class LlmWithRagResponse
{
    public int Id { get; set; }
    public Guid SessionId { get; set; }
    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public float ElapsedTime { get; set; }
    public InstructionEntity Instruction { get; set; } = default!;
    public EvaluationSession EvaluationSession { get; set; } = default!;
    public DateTime CreationDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
