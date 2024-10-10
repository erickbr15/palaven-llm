namespace Palaven.Model;

public class BlobStorageOptions
{
    public static readonly string SectionName = "BlobStorage";
    public IDictionary<string, string> Containers { get; set; } = new Dictionary<string, string>();
}
