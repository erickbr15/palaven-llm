namespace Palaven.Model.Ingest.Documents.Golden;

public class TaxLawArticleSummary
{
    public string Summary { get; set; } = default!;
    public IList<double> Embeddings { get; set; } = default!;
    public FineTuningInstructionMetadata Metadata { get; set; } = default!;
}
