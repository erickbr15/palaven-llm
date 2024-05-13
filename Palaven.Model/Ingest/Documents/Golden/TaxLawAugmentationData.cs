namespace Palaven.Model.Ingest.Documents.Golden;

public class TaxLawAugmentationData
{
    public TaxLawArticleSummary Summary { get; set; } = default!;
    public IList<TaxLawArticleRagQuestion> Questions { get; set; } = new List<TaxLawArticleRagQuestion>();
}
