namespace Palaven.Infrastructure.Model.Messaging;

public class CurateArticlesMessage
{
    public string OperationId { get; set; } = default!;
    public string[] DocumentIds { get; set; } = default!;
}
