namespace Palaven.Model.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseModel
{
    public string ChatCompletionExcerciseType { get; set; } = default!;
    public Guid SessionId { get; set; }
    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
}
