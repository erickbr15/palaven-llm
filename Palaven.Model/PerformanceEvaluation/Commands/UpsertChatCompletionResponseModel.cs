using Newtonsoft.Json;

namespace Palaven.Model.PerformanceEvaluation.Commands;

public class UpsertChatCompletionResponseModel
{
    [JsonIgnore]
    public string? ChatCompletionExcerciseType { get; set; } = default!;

    [JsonIgnore]
    public Guid? SessionId { get; set; }

    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
}
