namespace Palaven.Model.Datasets;

public class Instruction
{
    public int Id { get; set; }
    public string InstructionRequest { get; set; } = default!;
    public string Response { get; set; } = default!;
    public string Category { get; set; } = default!;
    public Guid GoldenArticleId { get; set; }
    public Guid? LawId { get; set; }
    public Guid? ArticleId { get; set; }
}
