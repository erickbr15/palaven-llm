namespace Palaven.Infrastructure.Model.Persistence.Entities;

public class InstructionEntity
{    
    public Guid InstructionId { get; set; }
    public Guid DatasetId { get; set; }
    public string Instruction { get; set; } = default!;
    public string Response { get; set; } = default!;
    public string Category { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }    
}