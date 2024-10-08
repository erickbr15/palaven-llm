namespace Palaven.Model.Data.Entities;

public class InstructionEntity
{
    public int Id { get; set; }
    public Guid DatasetId { get; set; }
    public string Instruction { get; set; } = default!;
    public string Response { get; set; } = default!;
    public string Category { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
    public Guid? LawId { get; set; }
    public Guid? ArticleId { get; set; }
}