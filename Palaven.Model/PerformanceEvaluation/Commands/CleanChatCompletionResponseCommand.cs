namespace Palaven.Model.PerformanceEvaluation.Commands;

public sealed class CleanChatCompletionResponseCommand
{
    public Guid SessionId { get; set;}
    public int BatchNumber { get; set; }
    public string ChatCompletionExcerciseType { get; set; } = default!;
}
