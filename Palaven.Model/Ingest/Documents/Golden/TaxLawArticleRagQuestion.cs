namespace Palaven.Model.Ingest.Documents.Golden;

public class TaxLawArticleRagQuestion
{
    public string Question { get; set; } = default!;
    public IList<double> Embeddings { get; set; } = default!;
    public FineTuningInstructionMetadata Metadata { get; set; } = default!;
}
