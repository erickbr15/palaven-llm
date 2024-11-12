namespace Palaven.Infrastructure.Model.AI.Llm;

public class RetrievalOptions
{    
    public string? Namespace { get; set; }    
    public int TopK { get; set; }
    public bool IncludeValues { get; set; }
    public double MinimumMatchScore { get; set; }
}
