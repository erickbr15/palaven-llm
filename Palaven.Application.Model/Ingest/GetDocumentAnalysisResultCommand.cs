namespace Palaven.Application.Model.Ingest;

public class GetDocumentAnalysisResultCommand
{
    public Guid OperationId { get; set; }
    public string DocumentAnalysisOperationId { get; set; } = default!;
}
