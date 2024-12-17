namespace Palaven.Application.Model.DatasetManagement;

public class FineTuningPromptData
{
    public Guid PromptId { get; set; }
    public Guid InstructionId { get; set; }
    public int ChunckNumber { get; set; }
    public Guid DatasetId { get; set; }
    public string Prompt { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
    public Guid? LawId { get; set; }
    public Guid? ArticleId { get; set; }
}
