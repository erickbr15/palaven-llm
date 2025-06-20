﻿using Newtonsoft.Json;

namespace Palaven.Application.Model.PerformanceEvaluation;

public class ChatCompletionResponse
{    
    [JsonIgnore]
    public Guid? SessionId { get; set; }

    [JsonIgnore]
    public string EvaluationExercise { get; set; } = default!;

    public int BatchNumber { get; set; }
    public Guid InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public float? ElapsedTime { get; set; }
}
