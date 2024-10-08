namespace Palaven.Model.Data.Entities;

public class FineTuningPromptEntity
{
    public int PromptId { get; set; }
    public int InstructionId { get; set; }
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string Prompt { get; set; } = default!;

    public InstructionEntity Instruction { get; set; } = default!;
}