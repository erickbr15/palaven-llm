namespace Palaven.Model.PerformanceEvaluation;

public class LlmChatCompletionResponseQuery
{
    public Func<LlmResponseView, bool> SelectionCriteria { get; set; } = default!;
}
