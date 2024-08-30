namespace Palaven.Model.PerformanceEvaluation;

public class LlmChatCompletionResponseQuery
{
    public Func<LlmResponseView, bool> SelectionCriteria { get; set; } = default!;
    public string ChatCompletionExcerciseType { get; set; } = default!;
}
