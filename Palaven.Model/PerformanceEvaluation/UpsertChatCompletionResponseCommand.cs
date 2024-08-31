namespace Palaven.Model.PerformanceEvaluation;

public class UpsertChatCompletionResponseCommand
{
    public IEnumerable<ChatCompletionResponse> ChatCompletionResponses { get; set; } = default!;
}
