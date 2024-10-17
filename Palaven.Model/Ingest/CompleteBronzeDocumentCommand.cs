namespace Palaven.Model.Ingest;

public class CompleteBronzeDocumentCommand
{
    public Guid OperationId { get; set; }
    public string DocumentAnalysisOperationId { get; set; } = default!;

}
