namespace Palaven.Application.Model.Ingest;

public class CurateArticlesCommand
{
    public Guid OperationId { get; set; }
    public Guid[] DocumentIds { get; set; } = default!;
}
