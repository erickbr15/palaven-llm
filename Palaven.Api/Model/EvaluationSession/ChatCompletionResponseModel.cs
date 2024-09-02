namespace Palaven.Api.Model.EvaluationSession;

public class ChatCompletionResponseModel
{
    public int BatchNumber { get; set; }
    public int InstructionId { get; set; }
    public string? ResponseCompletion { get; set; }
    public float? ElapsedTime { get; set; }
}
