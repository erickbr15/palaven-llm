namespace Palaven.Model.Ingest;

public class TaxLawDocumentIngestTask : IIngestTask
{
    public Guid TraceId { get; set; }
    public bool IsCompleted { get; set; }
}
