using Palaven.Model.Datasets;

namespace Palaven.Model.LllmPerformance;

public class RagBertScoreEvaluationMetric
{
    public int Id { get; set; }
    public int InstructionId { get; set; }
    public string LargeLanguageModel { get; set; } = default!;
    public string? Language { get; set; }
    public string? ResponseCompletion { get; set; }
    public float? BertScorePrecision { get; set; }
    public float? BertScoreRecall { get; set; }
    public float? BertScoreF1 { get; set; }    
    public Instruction InstructionRequest { get; set; } = default!;
}
