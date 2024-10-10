namespace Palaven.Model;

public class BlobStorageOptions
{
    public IDictionary<string, string> Containers { get; set; } = new Dictionary<string, string>();
}
