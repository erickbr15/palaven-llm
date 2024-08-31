namespace Palaven.Model.PerformanceEvaluation;

public class LlmResponseView
{
    public Guid EvaluationSessionId { get; set; }
    public Guid DatasetId { get; set; }
    public string EvaluationExercise { get; set; } = default!;
    public int BatchSize { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string DeviceInfo { get; set; } = default!;
    public string ChatCompletionExcerciseType { get; set; } = default!;
    public int InstructionId { get; set; }
    public int BatchNumber { get; set; }
    public string Instruction { get; set; } = default!;
    public string Response { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string? LlmResponseToEvaluate { get; set; }
    public float? ElapsedTime { get; set; }
}
