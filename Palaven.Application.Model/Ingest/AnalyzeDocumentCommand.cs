namespace Palaven.Application.Model.Ingest;

public class AnalyzeDocumentCommand
{
    public Guid OperationId { get; set; }
    public Stream DocumentContent { get; set; } = default!;
    public string DocumentLocale { get; set; } = default!;
    public IList<string> DocumentPages { get; set; } = new List<string>();
}
