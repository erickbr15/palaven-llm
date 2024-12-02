namespace Palaven.Infrastructure.Model.Messaging;

public class ExtractArticleParagraphsMessage
{
    public string OperationId { get; set; } = default!;
    public string TenantId { get; set; } = default!;
}
