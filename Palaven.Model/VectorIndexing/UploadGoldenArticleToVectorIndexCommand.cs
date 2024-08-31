namespace Palaven.Model.VectorIndexing;

public class UploadGoldenArticleToVectorIndexCommand
{
    public Guid TraceId { get; set; }
    public Guid GoldenArticleId { get; set; }
}
