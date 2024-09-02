namespace Palaven.Model.PerformanceEvaluation;

public static class ChatCompletionExcerciseType
{
    public const string LlmVanilla = "llmvanilla";
    public const string LlmWithRag = "llmrag";
    public const string LlmFineTuned= "llmfinetuned";
    public const string LlmFineTunedAndRag = "llmfinetunedrag";

    public static bool IsValid(string chatCompletionEvaluationExcercise)
    {
        if(string.IsNullOrWhiteSpace(chatCompletionEvaluationExcercise))
        {
            return false;
        }
       
        return chatCompletionEvaluationExcercise.ToLower() switch
        {
            LlmVanilla => true,
            LlmWithRag => true,
            LlmFineTuned => true,
            LlmFineTunedAndRag => true,
            _ => false
        };
    }

    public static string GetChatCompletionExcerciseTypeDescription(int chatCompletionEvaluationExcercise)
    {
        return chatCompletionEvaluationExcercise switch
        {
            1 => LlmVanilla,
            2 => LlmWithRag,
            3 => LlmFineTuned,
            4 => LlmFineTunedAndRag,
            _ => "Unknown"
        };
    }
}