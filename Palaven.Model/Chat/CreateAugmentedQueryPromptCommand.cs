namespace Palaven.Model.Chat;

public class CreateAugmentedQueryPromptCommand
{
    public Guid UserId { get; set; }
    public string Query { get; set; } = default!;
    public int TopK { get; set; }
    public float MinMatchScore { get; set; }
}
