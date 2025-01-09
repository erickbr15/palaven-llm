namespace Palaven.Application.Model.DatasetManagement;

public class FineTuningPromptData
{
    public Guid PromptId { get; set; }
    public Guid InstructionId { get; set; }
    public Guid DatasetId { get; set; }
    public string Prompt { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
}
