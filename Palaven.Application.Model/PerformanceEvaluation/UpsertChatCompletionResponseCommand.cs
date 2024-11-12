namespace Palaven.Application.Model.PerformanceEvaluation;

public class UpsertChatCompletionResponseCommand
{
    public IEnumerable<ChatCompletionResponse> ChatCompletionResponses { get; set; } = default!;
}
