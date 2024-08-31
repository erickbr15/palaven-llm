namespace Palaven.Model.PerformanceEvaluation;

public class FineTuningPromptData
{
    public int PromptId { get; set; }
    public int InstructionId { get; set; }
    public int ChunckNumber { get; set; }
    public Guid DatasetId { get; set; }
    public string Prompt { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
    public Guid? LawId { get; set; }
    public Guid? ArticleId { get; set; }
}
