namespace Palaven.Model.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseCommand
{
    public IEnumerable<ChatCompletionResponse> ChatCompletionResponses { get; set; } = default!;
}
