namespace Palaven.Infrastructure.Model.Storage;

public class BlobContent
{
    public BinaryData BinaryData { get; set; } = default!;
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
