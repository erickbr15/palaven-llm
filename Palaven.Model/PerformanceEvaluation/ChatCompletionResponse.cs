using Newtonsoft.Json;

namespace Palaven.Model.PerformanceEvaluation;

public class ChatCompletionResponse
{    
    [JsonIgnore]
    public Guid? SessionId { get; set; }

    [JsonIgnore]
    public string EvaluationExercise { get; set; } = default!;

    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public float? ElapsedTime { get; set; }
}
