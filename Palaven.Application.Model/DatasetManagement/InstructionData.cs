namespace Palaven.Application.Model.DatasetManagement;

public class InstructionData
{
    public int InstructionId { get; set; }
    public int ChunckNumber { get; set; }
    public Guid DatasetId { get; set; }
    public string Instruction { get; set; } = default!;
    public string Response { get; set; } = default!;
    public string Category { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
    public Guid? LawId { get; set; }
    public Guid? ArticleId { get; set; }
}
