namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class FineTuningPromptEntity
{
    public Guid PromptId { get; set; }
    public Guid InstructionId { get; set; }
    public Guid DatasetId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string Prompt { get; set; } = default!;

    public InstructionEntity Instruction { get; set; } = default!;
}