namespace Palaven.Model.PerformanceEvaluation;

public class RougePerformanceEvaluationModel
{
    public string RougeType { get; set; } = default!;
    public float? RougeScorePrecision { get; set; }
    public float? RougeScoreRecall { get; set; }
    public float? RougeScoreF1 { get; set; }
}
