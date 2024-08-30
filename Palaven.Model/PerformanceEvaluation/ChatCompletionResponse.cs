using Newtonsoft.Json;

namespace Palaven.Model.PerformanceEvaluation;

public class ChatCompletionResponse
{
    [JsonIgnore]
    public string? ChatCompletionExcerciseType { get; set; } = default!;

    [JsonIgnore]
    public Guid? SessionId { get; set; }

    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public float? ElapsedTime { get; set; }
}
