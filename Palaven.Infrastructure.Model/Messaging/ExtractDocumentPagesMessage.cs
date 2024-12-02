namespace Palaven.Infrastructure.Model.Messaging;

public class ExtractDocumentPagesMessage
{
    public string OperationId { get; set; } = default!;
    public string TenantId { get; set; } = default!;
    public string DocumentAnalysisOperationId { get; set; } = default!;
}
