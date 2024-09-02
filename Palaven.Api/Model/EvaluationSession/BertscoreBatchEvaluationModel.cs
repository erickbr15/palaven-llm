namespace Palaven.Api.Model.EvaluationSession;

public class BertScoreBatchEvaluationModel
{
    public int BatchNumber { get; set; }
    public float? Precision { get; set; }
    public float? Recall { get; set; }
    public float? F1 { get; set; }
}
