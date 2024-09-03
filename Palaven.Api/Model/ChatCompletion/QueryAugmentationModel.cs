namespace Palaven.Api.Model.ChatCompletion;

public class QueryAugmentationModel
{
    public int TopK { get; set; }
    public float MinMatchScore { get; set; }
    public string Query { get; set; } = default!;
}
