namespace Palaven.Model.PerformanceEvaluation.Commands;

public class SearchLlmChatCompletionResponseCriteria
{
    public Func<LlmResponseView, bool> SelectionCriteria { get; set; } = default!;
    public string ChatCompletionExcerciseType { get; set; } = default!;
}
