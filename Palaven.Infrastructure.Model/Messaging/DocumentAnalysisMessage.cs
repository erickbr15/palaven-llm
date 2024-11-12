
namespace Palaven.Infrastructure.Model.Messaging;

public class DocumentAnalysisMessage
{
    public string OperationId { get; set; } = default!;
    public string DocumentBlobName { get; set; } = default!;
    public string Locale { get; set; } = default!;
    public IList<string> Pages { get; set; } = new List<string>();

}
